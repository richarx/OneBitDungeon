using System;
using Enemies.Scripts.Behaviours;
using Player.Scripts;
using PrimeTween;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Tools_and_Scripts;
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

    [NonSerialized] private Sequence attackSequence;
    [NonSerialized] private Sequence outcomeSequence;
    [NonSerialized] private RectangleDamageZone currentDamageZone;
    [NonSerialized] private Transform currentDamageZoneRoot;
    [NonSerialized] private CloseDodgeSession closeDodgeSession;
    [NonSerialized] private BiscottoArrogance biscottoArrogance;
    [NonSerialized] private EnemyController currentEnemy;
    [NonSerialized] private BehaviourExecution currentExecution;
    [NonSerialized] private float aimEndTimestamp;
    [NonSerialized] private bool outcomeWasResolved;

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

        if (!biscottoArrogance.IsFull)
        {
            execution.Complete();
            return;
        }

        if (data.RectangularDamageZonePrefab == null)
        {
            Debug.LogError("[BiscottoReversBehaviour] Un prefab de zone rectangulaire est requis.", enemy);
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

        float timeToDamage = data.SpawnDuration + data.FillDuration + FilledColorTransitionDuration;
        float impactLeadTime = Mathf.Min(data.ImpactAnimationLeadTime, timeToDamage);

        attackSequence = Sequence.Create()
            .ChainCallback(() =>
            {
                PlayAnimation(enemy, data.InvitationAnimation);
                SpawnDamageZone(enemy);
            })
            .ChainDelay(timeToDamage - impactLeadTime)
            .ChainCallback(() => PlayAnimation(enemy, data.ImpactAnimation))
            .ChainDelay(impactLeadTime + DamageFlashDuration);
    }

    public void UpdateBehaviour(EnemyController enemy)
    {
        if (currentDamageZoneRoot == null || Time.time >= aimEndTimestamp)
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
        GameObject zoneObject = UnityEngine.Object.Instantiate(
            data.RectangularDamageZonePrefab,
            enemy.transform.position,
            Quaternion.identity);

        currentDamageZoneRoot = zoneObject.transform;
        currentDamageZone = zoneObject.GetComponentInChildren<RectangleDamageZone>();

        if (currentDamageZone == null)
        {
            Debug.LogError("[BiscottoReversBehaviour] Le prefab ne contient pas de RectangleDamageZone.", zoneObject);
            UnityEngine.Object.Destroy(zoneObject);
            CompleteWithoutOutcome();
            return;
        }

        float timeToDamage = data.SpawnDuration + data.FillDuration + FilledColorTransitionDuration;
        aimEndTimestamp = Time.time + Mathf.Max(0.0f, timeToDamage - data.LockBeforeImpact);

        RotateZoneTowardPlayer(enemy, true);
        currentDamageZone.Setup(Vector2.right, data.SpawnDuration, data.FillDuration, closeDodgeSession);
    }

    private void RotateZoneTowardPlayer(EnemyController enemy, bool immediate = false)
    {
        if (currentDamageZoneRoot == null || PlayerStateMachine.instance == null)
            return;

        Vector3 direction = PlayerStateMachine.instance.position - enemy.transform.position;
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

        currentDamageZoneRoot.rotation = Quaternion.Slerp(
            currentDamageZoneRoot.rotation,
            targetRotation,
            Time.deltaTime / Mathf.Max(0.001f, data.RotationDampening));
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

        if (outcomeSequence.isAlive)
            outcomeSequence.Stop();

        UnsubscribeFromSession();
        closeDodgeSession?.Cancel();

        if (currentDamageZone != null)
            currentDamageZone.Cancel();

        attackSequence = default;
        outcomeSequence = default;
        currentDamageZone = null;
        currentDamageZoneRoot = null;
        closeDodgeSession = null;
        biscottoArrogance = null;
        currentEnemy = null;
        currentExecution = null;
        aimEndTimestamp = 0.0f;
        outcomeWasResolved = false;
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
