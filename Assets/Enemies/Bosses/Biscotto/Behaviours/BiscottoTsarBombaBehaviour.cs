using System;
using Enemies.Scripts.Behaviours;
using Player.Scripts;
using PrimeTween;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Tools_and_Scripts;
using UnityEngine;

[Serializable]
public sealed class BiscottoTsarBombaBehaviour : IEnemyBehaviour
{
    private const float DamageColorTransitionDuration = 0.05f;

    [OdinSerialize]
    [Required]
    [LabelText("Data")]
    private BiscottoTsarBombaData data;

    private Sequence attackSequence;
    private Sequence jumpSequence;
    private CircleDamageZone currentDamageZone;
    private bool isTrackingPlayer;
    private Vector3 lockedLandingPosition;
    private Vector3 lockedFlyingPosition;
    private float startAttackTimestamp;

    private const float jumpHeight = 15.0f;


    public void StartBehaviour(EnemyController enemy, BehaviourExecution execution)
    {
        ResetRuntimeState(enemy, false);

        if (data == null)
        {
            Debug.LogError("[BiscottoTsarBombaBehaviour] Un data Tsar Bomba est requis.", enemy);
            execution.Complete();
            return;
        }

        if (data.CircleDamageZonePrefab == null)
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

        startAttackTimestamp = Time.time;

        attackSequence = Sequence.Create()
            .ChainCallback(() => StartAscent(enemy, data.AscentDuration))
            .ChainCallback(() => StartDamageZoneTracking(enemy))
            .ChainDelay(data.SpawnDuration + data.FillDuration - data.FallDuration)
            .ChainCallback(() => PlayAnimation(enemy, data.JumpAnimation))
            .ChainCallback(() => StartDescent(enemy, data.FallDuration))
            .ChainDelay(data.FallDuration)
            .ChainCallback(() => CompleteLanding(enemy))
            .ChainDelay(data.RecoveryDuration)
            .ChainCallback(() => PlayAnimation(enemy, "Idle"))
            .ChainCallback(() => execution.Complete());
    }

    public void UpdateBehaviour(EnemyController enemy)
    {
        if (!isTrackingPlayer || currentDamageZone == null || PlayerStateMachine.instance == null)
            return;

        Vector3 targetPosition = PlayerStateMachine.instance.position;
        currentDamageZone.transform.position = new Vector3(targetPosition.x, 0.0f, targetPosition.z);

        if (isTrackingPlayer && Time.time - startAttackTimestamp >= data.TrackingDuration)
        {
            isTrackingPlayer = false;
            Vector3 target = currentDamageZone.transform.position;
            lockedLandingPosition = new Vector3(target.x, 0.0f, target.z);
            lockedFlyingPosition = new Vector3(target.x, jumpHeight, target.z);
        }
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

    private void StartAscent(EnemyController enemy, float ascentDuration)
    {
        PlayAnimation(enemy, data.AnticipationAnimation);
        enemy.DeactivateHitbox();

        jumpSequence = Sequence.Create()
            .ChainDelay(0.1f)
            .Chain(Tween.PositionY(enemy.transform, jumpHeight, ascentDuration, Ease.OutQuad));
    }

    private void StartDescent(EnemyController enemy, float descentDuration)
    {
        if (jumpSequence.isAlive)
            jumpSequence.Stop();

        jumpSequence = Sequence.Create()
            .Chain(Tween.Position(enemy.transform, lockedFlyingPosition, lockedLandingPosition, descentDuration, Ease.OutQuad));
    }

    private void CompleteLanding(EnemyController enemy)
    {
        if (jumpSequence.isAlive)
            jumpSequence.Stop();

        enemy.transform.position = lockedLandingPosition;
        PlayAnimation(enemy, data.ImpactAnimation);
        enemy.GetComponent<SqueezeAndStretch>().Trigger();
        RestoreEnemyHitbox(enemy);
    }

    private void StartDamageZoneTracking(EnemyController enemy)
    {
        if (PlayerStateMachine.instance == null)
            return;

        Vector3 zonePosition = new Vector3(PlayerStateMachine.instance.position.x, 0.0f, PlayerStateMachine.instance.position.z);
        currentDamageZone = UnityEngine.Object.Instantiate(
            data.CircleDamageZonePrefab,
            zonePosition,
            Quaternion.Euler(90.0f, 0.0f, 0.0f));

        currentDamageZone.Setup(data.Radius, data.SpawnDuration, data.FillDuration);
        isTrackingPlayer = true;
    }

    private void ResetRuntimeState(EnemyController enemy, bool restoreGroundPosition)
    {
        if (attackSequence.isAlive)
            attackSequence.Stop();

        if (jumpSequence.isAlive)
            jumpSequence.Stop();

        if (currentDamageZone != null && !currentDamageZone.IsDestroyed)
            currentDamageZone.Cancel();

        if (restoreGroundPosition && enemy != null)
        {
            Vector3 position = enemy.transform.position;
            position.y = 0.0f;
            enemy.transform.position = position;
        }

        RestoreEnemyHitbox(enemy);

        attackSequence = default;
        jumpSequence = default;
        currentDamageZone = null;
        isTrackingPlayer = false;
    }

    private void RestoreEnemyHitbox(EnemyController enemy)
    {
        if (enemy.damageable == null || !enemy.damageable.IsDead)
            enemy.ActivateHitbox();
    }

    private static void PlayAnimation(EnemyController enemy, string animationName)
    {
        if (enemy.animator != null && !string.IsNullOrWhiteSpace(animationName))
            enemy.animator.Play(animationName);
    }
}
