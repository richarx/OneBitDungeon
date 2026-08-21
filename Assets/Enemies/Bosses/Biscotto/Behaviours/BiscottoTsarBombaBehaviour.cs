using System;
using Enemies.Scripts.Behaviours;
using Player.Scripts;
using PrimeTween;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
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
    private float groundY;
    private Vector3 lockedLandingPosition;

    public void StartBehaviour(EnemyController enemy, BehaviourExecution execution)
    {
        ResetRuntimeState(enemy, false);
        groundY = enemy.transform.position.y;

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

        float timeToDamage = data.SpawnDuration + data.FillDuration;
        float effectiveLockDuration = Mathf.Min(data.LockBeforeImpact, timeToDamage);
        float trackingDuration = Mathf.Max(0.0f, timeToDamage - effectiveLockDuration);

        attackSequence = Sequence.Create()
            .ChainCallback(() =>
            {
                PlayAnimation(enemy, data.AnticipationAnimation);
                SpawnTrackingZone(enemy);
            })
            .ChainDelay(trackingDuration)
            .ChainCallback(() => LockTargetAndJump(enemy, effectiveLockDuration))
            .ChainDelay(effectiveLockDuration)
            .ChainCallback(() => FinishLanding(enemy))
            .ChainDelay(data.RecoveryDuration)
            .ChainCallback(() => execution.Complete());
    }

    public void UpdateBehaviour(EnemyController enemy)
    {
        if (!isTrackingPlayer || currentDamageZone == null || PlayerStateMachine.instance == null)
            return;

        currentDamageZone.transform.position = new Vector3(PlayerStateMachine.instance.position.x, groundY, PlayerStateMachine.instance.position.z);
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

    private void SpawnTrackingZone(EnemyController enemy)
    {
        Vector3 zonePosition = new Vector3(PlayerStateMachine.instance.position.x, groundY, PlayerStateMachine.instance.position.z);
        currentDamageZone = UnityEngine.Object.Instantiate(
            data.CircleDamageZonePrefab,
            zonePosition,
            Quaternion.Euler(90.0f, 0.0f, 0.0f));

        currentDamageZone.Setup(data.Radius, data.SpawnDuration, data.FillDuration);
        isTrackingPlayer = true;
    }

    private void LockTargetAndJump(EnemyController enemy, float jumpDuration)
    {
        isTrackingPlayer = false;

        Vector3 target = currentDamageZone != null
            ? currentDamageZone.transform.position
            : PlayerStateMachine.instance.position;

        lockedLandingPosition = new Vector3(target.x, groundY, target.z);
        PlayAnimation(enemy, data.JumpAnimation);

        enemy.DeactivateHitbox();
        enemyHitboxIsDisabled = true;

        if (jumpDuration <= 0.0f)
        {
            enemy.transform.position = lockedLandingPosition;
            return;
        }

        Vector3 startPosition = enemy.transform.position;
        Vector3 apexPosition = Vector3.Lerp(startPosition, lockedLandingPosition, 0.5f) + Vector3.up * data.JumpHeight;
        float ascentDuration = jumpDuration * data.RatioAscentToFall;
        float descentDuration = jumpDuration - ascentDuration;

        jumpSequence = Sequence.Create()
            .Chain(Tween.Custom(0.0f, 1.0f, ascentDuration, progress =>
            {
                enemy.transform.position = GetJumpAscentPosition(startPosition, apexPosition, progress);
            }, Ease.Linear))
            .Chain(Tween.Position(enemy.transform, lockedLandingPosition, descentDuration, Ease.Linear));
    }

    private void FinishLanding(EnemyController enemy)
    {
        if (jumpSequence.isAlive)
            jumpSequence.Stop();

        enemy.transform.position = lockedLandingPosition;
        RestoreEnemyHitbox(enemy);
        PlayAnimation(enemy, data.ImpactAnimation);
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
            position.y = groundY;
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
