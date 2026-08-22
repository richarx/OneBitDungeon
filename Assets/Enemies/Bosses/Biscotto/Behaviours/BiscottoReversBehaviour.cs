using System;
using Enemies.Scripts.Behaviours;
using Player.Scripts;
using PrimeTween;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

[Serializable]
public sealed class BiscottoReversBehaviour : IEnemyBehaviour, IConditionalEnemyBehaviour
{
    private const float FilledColorTransitionDuration = 0.05f;
    private const float DamageFlashDuration = 0.05f;

    [OdinSerialize]
    [Required]
    [LabelText("Data")]
    private BiscottoReversData data;

    private Sequence attackSequence;
    private Sequence moveSequence;
    private Sequence outcomeSequence;
    private ConeDamageZone currentDamageZone;
    private CloseDodgeSession closeDodgeSession;
    private BiscottoArrogance biscottoArrogance;
    private EnemyController currentEnemy;
    private BehaviourExecution currentExecution;
    private float aimEndTimestamp;
    private bool outcomeWasResolved;
    private Vector2 currentAimDirection;

    public void StartBehaviour(EnemyController enemy, BehaviourExecution execution)
    {
        ResetRuntimeState();

        currentEnemy = enemy;
        currentExecution = execution;
        biscottoArrogance = enemy.GetComponent<BiscottoArrogance>();

        if (data == null)
        {
            Debug.LogError("[BiscottoReversBehaviour] Un data Revers est requis.", enemy);
            execution.Complete();
            return;
        }

        if (biscottoArrogance == null)
        {
            Debug.LogError("[BiscottoReversBehaviour] BiscottoArrogance est requis sur le boss.", enemy);
            execution.Complete();
            return;
        }

        if (!biscottoArrogance.IsFull && !execution.DebugMode)
        {
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

        closeDodgeSession = new CloseDodgeSession(1);
        closeDodgeSession.OnCompleted += HandleOutcome;

        float timeToDamage = data.SpawnDuration + data.FillDuration;

        attackSequence = Sequence.Create()
            .ChainCallback(() =>
            {
                StartMove(enemy);
            })
            .ChainDelay(data.MoveDuration)
            .ChainCallback(() =>
            {
                PlayAnimation(enemy, data.InvitationAnimation);
                SpawnDamageZone(enemy);
            })
            .ChainDelay(timeToDamage)
            .ChainCallback(() => PlayAnimation(enemy, data.ImpactAnimation))
            .ChainDelay(DamageFlashDuration);
    }

    public void UpdateBehaviour(EnemyController enemy)
    {
        if (currentDamageZone == null || currentDamageZone.IsDestroyed || Time.time >= aimEndTimestamp)
            return;

        RotateZoneTowardPlayer(enemy);
    }

    public void FixedUpdateBehaviour(EnemyController enemy)
    {
    }

    public void StopBehaviour(EnemyController enemy)
    {
        ResetRuntimeState();
    }

    public void CancelBehaviour(EnemyController enemy)
    {
        ResetRuntimeState();
    }

    public void SetSubBehaviourState(bool state)
    {
    }

    public bool CanExecute(EnemyController enemy)
    {
        BiscottoArrogance arrogance = enemy != null ? enemy.GetComponent<BiscottoArrogance>() : null;
        return arrogance != null && arrogance.IsFull;
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
            data.FillDuration,
            closeDodgeSession);
    }

    private void StartMove(EnemyController enemy)
    {
        if (PlayerStateMachine.instance == null || data.MoveDuration <= 0.0f)
            return;

        if (moveSequence.isAlive)
            moveSequence.Stop();

        Vector3 pivotPosition = PlayerStateMachine.instance.position;
        pivotPosition.y = enemy.transform.position.y;
        Vector3 destination = BiscottoMovementUtility.ComputeDestination(
            enemy.transform,
            PlayerStateMachine.instance.position,
            data.MoveDistance);

        if (data.TriggerAfterImageOnSideMove && enemy.afterImage != null)
            enemy.afterImage.Trigger(data.MoveDuration);

        moveSequence = BiscottoMovementUtility.CreateArcMove(
            enemy.transform,
            destination,
            pivotPosition,
            data.MoveDuration);
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

    private void HandleOutcome(CloseDodgeSessionOutcome outcome)
    {
        if (outcomeWasResolved)
            return;

        outcomeWasResolved = true;
        UnsubscribeFromSession();

        if (currentEnemy == null || currentExecution == null || !currentEnemy.IsExecutionActive(currentExecution))
            return;

        PlayerArrogance playerArrogance = PlayerStateMachine.instance != null
            ? PlayerStateMachine.instance.playerArrogance
            : null;

        switch (outcome)
        {
            case CloseDodgeSessionOutcome.Hit:
                biscottoArrogance.ConsumeFullArrogance();
                playerArrogance?.ClearArrogance();
                PlayAnimation(currentEnemy, data.HitPlayerAnimation);
                CompleteAfterDelay(data.HitRecoveryDuration);
                break;

            case CloseDodgeSessionOutcome.CloseDodge:
                biscottoArrogance.ConsumeFullArrogance();
                playerArrogance?.FillArrogance();
                PlayAnimation(currentEnemy, data.ReflectedAnimation);
                ApplyReflectedDamage();

                if (currentEnemy.IsExecutionActive(currentExecution))
                    CompleteAfterDelay(data.ReflectedStunDuration);
                break;

            default:
                playerArrogance?.LoseArrogance(data.EarlyDodgeArroganceLoss);
                PlayAnimation(currentEnemy, data.EarlyDodgeAnimation);
                CompleteAfterDelay(data.EarlyDodgeRecoveryDuration);
                break;
        }
    }

    private void ApplyReflectedDamage()
    {
        if (data.SelfDamage <= 0 || currentEnemy.damageable == null)
            return;

        Vector3 direction = currentEnemy.transform.position - PlayerStateMachine.instance.position;
        currentEnemy.damageable.TakeDamage(data.SelfDamage, new Vector2(direction.x, direction.z).normalized);
    }

    private void CompleteAfterDelay(float delay)
    {
        if (outcomeSequence.isAlive)
            outcomeSequence.Stop();

        outcomeSequence = Sequence.Create()
            .ChainDelay(delay)
            .ChainCallback(() =>
            {
                if (currentEnemy != null && currentEnemy.IsExecutionActive(currentExecution))
                    currentExecution.Complete();
            });
    }

    private void CompleteWithoutOutcome()
    {
        UnsubscribeFromSession();
        closeDodgeSession?.Cancel();

        if (currentEnemy != null && currentEnemy.IsExecutionActive(currentExecution))
            currentExecution.Complete();
    }

    private void ResetRuntimeState()
    {
        if (attackSequence.isAlive)
            attackSequence.Stop();

        if (moveSequence.isAlive)
            moveSequence.Stop();

        if (outcomeSequence.isAlive)
            outcomeSequence.Stop();

        UnsubscribeFromSession();
        closeDodgeSession?.Cancel();

        if (currentDamageZone != null)
            currentDamageZone.Cancel();

        attackSequence = default;
        moveSequence = default;
        outcomeSequence = default;
        currentDamageZone = null;
        closeDodgeSession = null;
        biscottoArrogance = null;
        currentEnemy = null;
        currentExecution = null;
        aimEndTimestamp = 0.0f;
        outcomeWasResolved = false;
        currentAimDirection = Vector2.zero;
    }

    private void UnsubscribeFromSession()
    {
        if (closeDodgeSession != null)
            closeDodgeSession.OnCompleted -= HandleOutcome;
    }

    private static void PlayAnimation(EnemyController enemy, string animationName)
    {
        if (enemy.animator != null && !string.IsNullOrWhiteSpace(animationName))
            enemy.animator.Play(animationName);
    }
}
