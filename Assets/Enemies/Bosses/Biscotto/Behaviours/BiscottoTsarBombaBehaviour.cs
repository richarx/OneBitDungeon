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
    [LabelText("Prefab de zone circulaire")]
    private CircleDamageZone circleDamageZonePrefab;

    [OdinSerialize]
    [MinValue(0.0f)]
    [LabelText("Rayon")]
    private float radius = 0.16f;

    [OdinSerialize]
    [MinValue(0.0f)]
    [LabelText("Durée d'apparition")]
    private float spawnDuration = 0.3f;

    [OdinSerialize]
    [MinValue(0.0f)]
    [LabelText("Durée de suivi")]
    private float fillDuration = 0.9f;

    [OdinSerialize]
    [MinValue(0.0f)]
    [LabelText("Verrouillage avant impact")]
    [SuffixLabel("secondes")]
    private float lockBeforeImpact = 0.32f;

    [OdinSerialize]
    [MinValue(0.0f)]
    [LabelText("Hauteur du saut")]
    private float jumpHeight = 2.0f;

    [OdinSerialize]
    [MinValue(0.0f)]
    [LabelText("Hauteur de la zone")]
    private float zoneHeight = 0.06f;

    [OdinSerialize]
    [MinValue(0.0f)]
    [LabelText("Récupération")]
    private float recoveryDuration = 0.8f;

    [Title("Animations")]
    [OdinSerialize]
    [LabelText("Anticipation")]
    private string anticipationAnimation;

    [OdinSerialize]
    [LabelText("Saut")]
    private string jumpAnimation;

    [OdinSerialize]
    [LabelText("Impact")]
    private string impactAnimation;

    [NonSerialized] private Sequence attackSequence;
    [NonSerialized] private Sequence jumpSequence;
    [NonSerialized] private CircleDamageZone currentDamageZone;
    [NonSerialized] private bool isTrackingPlayer;
    [NonSerialized] private bool enemyHitboxIsDisabled;
    [NonSerialized] private float groundY;
    [NonSerialized] private Vector3 lockedLandingPosition;

    public void StartBehaviour(EnemyController enemy, BehaviourExecution execution)
    {
        ResetRuntimeState(enemy, false);
        groundY = enemy.transform.position.y;

        if (circleDamageZonePrefab == null)
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

        float timeToDamage = spawnDuration + fillDuration + DamageColorTransitionDuration;
        float effectiveLockDuration = Mathf.Min(lockBeforeImpact, timeToDamage);
        float trackingDuration = Mathf.Max(0.0f, timeToDamage - effectiveLockDuration);

        attackSequence = Sequence.Create()
            .ChainCallback(() =>
            {
                PlayAnimation(enemy, anticipationAnimation);
                SpawnTrackingZone(enemy);
            })
            .ChainDelay(trackingDuration)
            .ChainCallback(() => LockTargetAndJump(enemy, effectiveLockDuration))
            .ChainDelay(effectiveLockDuration)
            .ChainCallback(() => FinishLanding(enemy))
            .ChainDelay(recoveryDuration)
            .ChainCallback(() => execution.Complete());
    }

    public void UpdateBehaviour(EnemyController enemy)
    {
        if (!isTrackingPlayer || currentDamageZone == null || PlayerStateMachine.instance == null)
            return;

        currentDamageZone.transform.position = GetZonePosition(enemy, PlayerStateMachine.instance.position);
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
        Vector3 zonePosition = GetZonePosition(enemy, PlayerStateMachine.instance.position);
        currentDamageZone = UnityEngine.Object.Instantiate(
            circleDamageZonePrefab,
            zonePosition,
            Quaternion.Euler(90.0f, 0.0f, 0.0f));

        currentDamageZone.Setup(radius, spawnDuration, fillDuration);
        isTrackingPlayer = true;
    }

    private void LockTargetAndJump(EnemyController enemy, float jumpDuration)
    {
        isTrackingPlayer = false;

        Vector3 target = currentDamageZone != null
            ? currentDamageZone.transform.position
            : PlayerStateMachine.instance.position;

        lockedLandingPosition = new Vector3(target.x, groundY, target.z);
        PlayAnimation(enemy, jumpAnimation);

        enemy.DeactivateHitbox();
        enemyHitboxIsDisabled = true;

        if (jumpDuration <= 0.0f)
        {
            enemy.transform.position = lockedLandingPosition;
            return;
        }

        Vector3 startPosition = enemy.transform.position;
        Vector3 apexPosition = Vector3.Lerp(startPosition, lockedLandingPosition, 0.5f) + Vector3.up * jumpHeight;
        float halfDuration = jumpDuration * 0.5f;

        jumpSequence = Sequence.Create()
            .Chain(Tween.Position(enemy.transform, apexPosition, halfDuration, Ease.OutQuad))
            .Chain(Tween.Position(enemy.transform, lockedLandingPosition, halfDuration, Ease.InQuad));
    }

    private void FinishLanding(EnemyController enemy)
    {
        if (jumpSequence.isAlive)
            jumpSequence.Stop();

        enemy.transform.position = lockedLandingPosition;
        RestoreEnemyHitbox(enemy);
        PlayAnimation(enemy, impactAnimation);
    }

    private Vector3 GetZonePosition(EnemyController enemy, Vector3 targetPosition)
    {
        return new Vector3(targetPosition.x, groundY + zoneHeight, targetPosition.z);
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

    private static void PlayAnimation(EnemyController enemy, string animationName)
    {
        if (enemy.animator != null && !string.IsNullOrWhiteSpace(animationName))
            enemy.animator.Play(animationName);
    }
}
