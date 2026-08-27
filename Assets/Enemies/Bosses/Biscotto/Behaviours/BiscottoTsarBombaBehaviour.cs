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
    private bool enemyHitboxIsDisabled;
    private Vector3 lockedLandingPosition;

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

        float ascentDuration = data.AscentDuration;
        float trackingDuration = data.TrackingDuration;
        float fallDuration = data.LockBeforeImpact;

        attackSequence = Sequence.Create()
            .ChainCallback(() =>
            {
                StartAscent(enemy, ascentDuration);
            })
            .ChainDelay(ascentDuration)
            .ChainCallback(() => StartDamageZoneTracking(enemy))
            .ChainDelay(trackingDuration)
            .ChainCallback(() => LockTargetAndFall(enemy, fallDuration))
            .ChainDelay(fallDuration)
            .ChainCallback(() => RestoreEnemyHitbox(enemy))
            .ChainDelay(data.RecoveryDuration)
            .ChainCallback(() => PlayAnimation(enemy, "Idle"))
            .ChainCallback(() => execution.Complete());
    }

    public void UpdateBehaviour(EnemyController enemy)
    {
        if (!isTrackingPlayer || currentDamageZone == null || PlayerStateMachine.instance == null)
            return;

        currentDamageZone.transform.position = new Vector3(PlayerStateMachine.instance.position.x, 0.0f, PlayerStateMachine.instance.position.z);
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
        enemyHitboxIsDisabled = true;

        if (ascentDuration <= 0.0f)
            return;

        Vector3 startPosition = enemy.transform.position;
        Vector3 apexPosition = startPosition + Vector3.up * data.JumpHeight;

        jumpSequence = Sequence.Create()
            .Chain(Tween.Custom(0.0f, 1.0f, ascentDuration, progress =>
            {
                enemy.transform.position = GetJumpAscentPosition(startPosition, apexPosition, progress);
            }, Ease.OutQuad));
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

    private void LockTargetAndFall(EnemyController enemy, float fallDuration)
    {
        isTrackingPlayer = false;

        Vector3 target = currentDamageZone.transform.position;

        lockedLandingPosition = new Vector3(target.x, 0.0f, target.z);

        PlayAnimation(enemy, data.JumpAnimation);

        if (fallDuration <= 0.0f)
        {
            enemy.transform.position = lockedLandingPosition;
            return;
        }

        jumpSequence = Sequence.Create()
            .Chain(Tween.Position(enemy.transform, lockedLandingPosition, fallDuration, Ease.InQuad))
            .ChainCallback(() => enemy.transform.position = lockedLandingPosition)
            .ChainCallback(() => PlayAnimation(enemy, data.ImpactAnimation))
            .ChainCallback(() => enemy.GetComponent<SqueezeAndStretch>().Trigger());
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
        if (!enemyHitboxIsDisabled || enemy == null)
            return;

        enemyHitboxIsDisabled = false;

        if (enemy.damageable == null || !enemy.damageable.IsDead)
            enemy.ActivateHitbox();
    }

    private static Vector3 GetJumpAscentPosition(Vector3 startPosition, Vector3 apexPosition, float progress)
    {
        float clampedProgress = Mathf.Clamp01(progress);
        Vector3 position = Vector3.Lerp(startPosition, apexPosition, clampedProgress);
        position.y = Mathf.Lerp(startPosition.y, apexPosition.y, Mathf.Sin(clampedProgress * Mathf.PI * 0.5f));
        return position;
    }

    private static void PlayAnimation(EnemyController enemy, string animationName)
    {
        if (enemy.animator != null && !string.IsNullOrWhiteSpace(animationName))
            enemy.animator.Play(animationName);
    }
}
