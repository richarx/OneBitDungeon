using System;
using System.Collections;
using System.Collections.Generic;
using Enemies.Scripts.Behaviours;
using Player.Scripts;
using PrimeTween;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Tools_and_Scripts;
using UnityEngine;

public class PeacockDashAttackBehaviour : IEnemyBehaviour
{
    [OdinSerialize]
    [Required]
    [LabelText("Data")]
    private PeacockDashAttackData data;

    private Sequence attackSequence;
    private Sequence dashSequence;
    private RectangleDamageZone currentDamageZone;
    private Transform currentDamageZoneRoot;
    private float currentAimEndTimestamp;
    private const float DamageColorTransitionDuration = 0.05f;

    public void StartBehaviour(EnemyController enemy, BehaviourExecution execution)
    {
        if (data == null)
        {
            Debug.LogError("[BiscottoTsarBombaBehaviour] Un data Tsar Bomba est requis.", enemy);
            execution.Complete();
            return;
        }

        if (data.RectangularDamageZonePrefab == null)
        {
            Debug.LogError("[BiscottoTsarBombaBehaviour] Un prefab de zone circulaire est requis.", enemy);
            execution.Complete();
            return;
        }

        if (PlayerStateMachine.instance == null)
        {
            Debug.LogError("[BiscottoTsarBombaBehaviour] Aucun joueur n'est disponible.", enemy);
            execution.Complete();
            return;
        }

        attackSequence = Sequence.Create();

        if (data.MoveAwayFromPlayer)
        {
            attackSequence
                .ChainCallback(() =>
                {
                    if (data.TriggerAfterImageOnSideMove && enemy.afterImage != null)
                        enemy.afterImage.Trigger(data.MoveDuration);
                })
                .Chain(ComputeMovement(enemy, data.MoveDistance, data.MoveDuration));
        }

        attackSequence
            .ChainCallback(() =>
            {
                PlayAnimation(enemy, data.AnticipationAnimation);
                SpawnAttackZone(enemy);
            })
            .ChainDelay(data.SpawnDuration + data.FillDuration)
            .ChainCallback(() => DashTowardTarget(enemy))
            .ChainCallback(() => PlayAnimation(enemy, data.ImpactAnimation))
            .ChainDelay(data.DelayAfterImpact)
            .ChainDelay(data.FinalRecoveryDuration)
            .ChainCallback(() => PlayAnimation(enemy, "Idle"))
            .ChainCallback(() => execution.Complete());
    }

    private void DashTowardTarget(EnemyController enemy)
    {
        Vector3 targetPosition = currentDamageZoneRoot.position + currentDamageZoneRoot.right * data.DamageZoneLength;

        targetPosition = ClampPositionInArena(targetPosition);

        dashSequence = Sequence.Create()
            .Chain(Tween.Position(enemy.transform, targetPosition, data.DashDuration, Ease.OutBack));
    }

    private Tween ComputeMovement(EnemyController enemy, float moveDistance, float moveDuration)
    {
        Vector3 currentPosition = enemy.transform.position;
        Vector3 playerPosition = PlayerStateMachine.instance.position;

        Vector3 movementDirection = (currentPosition - playerPosition).normalized;
        Vector3 targetPosition = currentPosition + movementDirection * moveDistance;

        targetPosition = ClampPositionInArena(targetPosition);

        return Tween.Position(enemy.transform, targetPosition, moveDuration, Ease.OutCirc);
    }

    private Vector3 ClampPositionInArena(Vector3 position)
    {
        position.x = Mathf.Clamp(position.x, -9.0f, 9.0f);
        position.z = Mathf.Clamp(position.z, -9.0f, 9.0f);

        return position;
    }

    private void SpawnAttackZone(EnemyController enemy)
    {
        if (data.RectangularDamageZonePrefab == null)
        {
            Debug.LogError($"[{enemy.name}] Aucun prefab de zone n'est configuré pour l'attaque '{data.PatternName}'.", enemy);
            return;
        }

        GameObject zoneObject = UnityEngine.Object.Instantiate(data.RectangularDamageZonePrefab, enemy.transform.position, Quaternion.identity);
        RectangleDamageZone damageZone = zoneObject.GetComponentInChildren<RectangleDamageZone>();

        if (damageZone == null)
        {
            Debug.LogError($"[{enemy.name}] Le prefab '{data.RectangularDamageZonePrefab.name}' ne contient pas de RectangleDamageZone.", data.RectangularDamageZonePrefab);
            UnityEngine.Object.Destroy(zoneObject);
            return;
        }

        currentDamageZone = damageZone;
        currentDamageZoneRoot = zoneObject.transform;
        currentAimEndTimestamp = Time.time + Mathf.Max(
            0.0f,
            data.SpawnDuration + data.FillDuration + DamageColorTransitionDuration - data.LockBeforeImpact);

        damageZone.SetDimensions(data.DamageZoneWidth, data.DamageZoneLength);
        RotateCurrentZoneTowardPlayer(enemy, true);
        damageZone.Setup(Vector2.right, data.SpawnDuration, data.FillDuration);
    }

    private void RotateCurrentZoneTowardPlayer(EnemyController enemy, bool immediate = false)
    {
        if (currentDamageZoneRoot == null || PlayerStateMachine.instance == null)
            return;

        currentDamageZoneRoot.position = enemy.transform.position;

        Vector3 direction = PlayerStateMachine.instance.position - currentDamageZoneRoot.position;
        direction.y = 0.0f;

        if (direction.sqrMagnitude <= 0.0001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(
            direction.normalized.ToVector2().AddAngleToDirection(90.0f).ToVector3());

        if (immediate)
        {
            currentDamageZoneRoot.rotation = targetRotation;
            return;
        }

        float dampening = Mathf.Max(0.001f, data.RotationDampening);
        currentDamageZoneRoot.rotation = Quaternion.Slerp(
            currentDamageZoneRoot.rotation,
            targetRotation,
            Time.deltaTime / dampening);
    }

    public void UpdateBehaviour(EnemyController enemy)
    {
        if (currentDamageZoneRoot == null || Time.time >= currentAimEndTimestamp)
            return;

        RotateCurrentZoneTowardPlayer(enemy);
    }

    public void FixedUpdateBehaviour(EnemyController enemy)
    {
    }

    public void StopBehaviour(EnemyController enemy)
    {
        ResetRuntimeState(enemy, true);
    }

    public void CancelBehaviour(EnemyController enemy)
    {
        ResetRuntimeState(enemy, true);
    }

    public void SetSubBehaviourState(bool state)
    {
    }

    private void ResetRuntimeState(EnemyController enemy, bool restoreGroundPosition)
    {
        if (attackSequence.isAlive)
            attackSequence.Stop();

        if (dashSequence.isAlive)
            dashSequence.Stop();

        if (currentDamageZone != null)
            currentDamageZone.Cancel();

        if (restoreGroundPosition && enemy != null)
        {
            Vector3 position = enemy.transform.position;
            position.y = 0.0f;
            enemy.transform.position = position;
        }

        attackSequence = default;
        dashSequence = default;
        currentDamageZone = null;
    }


    private static void PlayAnimation(EnemyController enemy, string animationName)
    {
        if (enemy.animator != null && !string.IsNullOrWhiteSpace(animationName))
            enemy.animator.Play(animationName);
    }
}
