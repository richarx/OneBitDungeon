using System;
using System.Collections;
using System.Collections.Generic;
using Enemies.Scripts.Behaviours;
using Player.Scripts;
using PrimeTween;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

public class BiscottoSlapBehaviour : IEnemyBehaviour
{
    [OdinSerialize]
    [Required]
    [LabelText("Data")]
    private BiscottoSlapData data;

    private Sequence attackSequence;
    private ConeDamageZone currentDamageZone;
    private float aimEndTimestamp;
    private Vector2 currentAimDirection;

    private const float DamageFlashDuration = 0.05f;

    public void StartBehaviour(EnemyController enemy, BehaviourExecution execution)
    {
        ResetRuntimeState();

        if (data == null)
        {
            Debug.LogError("[BiscottoReversBehaviour] Un data Revers est requis.", enemy);
            execution.Complete();
            return;
        }

        if (data.ConeDamageZonePrefab == null)
        {
            Debug.LogError("[BiscottoReversBehaviour] Un prefab de zone conique est requis.", enemy);
            execution.Complete();
            return;
        }

        if (PlayerStateMachine.instance == null)
        {
            Debug.LogError("[BiscottoReversBehaviour] Aucun joueur n'est disponible.", enemy);
            execution.Complete();
            return;
        }

        float timeToDamage = data.SpawnDuration + data.FillDuration;

        attackSequence = Sequence.Create();

        attackSequence = ComputeMovement(enemy, attackSequence);

        attackSequence
            .ChainCallback(() => PlayAnimation(enemy, data.AnticipationAnimation))
            .ChainCallback(() => SpawnDamageZone(enemy))
            .ChainDelay(timeToDamage)
            .ChainCallback(() => PlayAnimation(enemy, data.ImpactAnimation))
            .ChainDelay(DamageFlashDuration)
            .ChainDelay(data.RecoveryDuration)
            .ChainCallback(() => execution.Complete());

    }

    private Sequence ComputeMovement(EnemyController enemy, Sequence attackSequence)
    {
        Vector3 targetPosition = ComputeTargetMovementPosition(enemy, data.MoveDistance);

        attackSequence
            .ChainCallback(() => PlayAnimation(enemy, "Idle"))
            .ChainCallback(() =>
            {
                if (data.TriggerAfterImageOnSideMove && enemy.afterImage != null)
                    enemy.afterImage.Trigger(data.MoveDuration);
            })
            .Chain(EnemyMovementUtility.CreateMoveToPosition(enemy, targetPosition, data.MoveDuration, Ease.OutCirc));

        return attackSequence;
    }

    private Vector3 ComputeTargetMovementPosition(EnemyController enemy, float moveDistance)
    {
        Vector3 position = PlayerStateMachine.instance.position;

        position.z += moveDistance;

        return ClampPositionInArena(position);
    }

    private Vector3 ClampPositionInArena(Vector3 position)
    {
        position.x = Mathf.Clamp(position.x, -9.0f, 9.0f);
        position.z = Mathf.Clamp(position.z, -9.0f, 9.0f);

        return position;
    }

    private void SpawnDamageZone(EnemyController enemy)
    {
        currentDamageZone = UnityEngine.Object.Instantiate(
            data.ConeDamageZonePrefab,
            enemy.transform.position,
            Quaternion.identity);

        float timeToDamage = data.SpawnDuration + data.FillDuration;
        aimEndTimestamp = Time.time + Mathf.Max(0.0f, timeToDamage - data.LockBeforeImpact);

        RotateZoneTowardPlayer(enemy, true);
        currentDamageZone.Setup(
            currentAimDirection,
            data.Radius,
            data.HalfAngle * 2.0f,
            data.SpawnDuration,
            data.FillDuration);
    }

    private void RotateZoneTowardPlayer(EnemyController enemy, bool immediate = false)
    {
        if (currentDamageZone == null || currentDamageZone.IsDestroyed || PlayerStateMachine.instance == null)
            return;

        Vector3 direction = PlayerStateMachine.instance.position - enemy.transform.position;
        direction.y = 0.0f;

        if (direction.sqrMagnitude <= 0.0001f)
            return;

        if (immediate)
        {
            currentAimDirection = new Vector2(direction.x, direction.z).normalized;
        }
        else
        {
            Vector2 targetDirection = new Vector2(direction.x, direction.z).normalized;
            currentAimDirection = Vector2.Lerp(
                currentAimDirection,
                targetDirection,
                Time.deltaTime / Mathf.Max(0.001f, data.RotationDampening)).normalized;
        }

        currentDamageZone.SetDirection(currentAimDirection);
    }

    public void UpdateBehaviour(EnemyController enemy)
    {
        //if (currentDamageZone != null && !currentDamageZone.IsDestroyed && Time.time < aimEndTimestamp)
        //  RotateZoneTowardPlayer(enemy);
    }

    public void FixedUpdateBehaviour(EnemyController enemy)
    {
    }

    public void CancelBehaviour(EnemyController enemy)
    {
        ResetRuntimeState();
    }

    public void StopBehaviour(EnemyController enemy)
    {
        ResetRuntimeState();
    }

    public void SetSubBehaviourState(bool state)
    {
    }

    private void ResetRuntimeState()
    {
        if (attackSequence.isAlive)
            attackSequence.Stop();

        if (currentDamageZone != null)
            currentDamageZone.Cancel();

        attackSequence = default;
        currentDamageZone = null;
        aimEndTimestamp = 0.0f;
        currentAimDirection = Vector2.zero;
    }

    private static void PlayAnimation(EnemyController enemy, string animationName)
    {
        if (enemy.animator != null && !string.IsNullOrWhiteSpace(animationName))
            enemy.animator.Play(animationName);
    }
}
