using System;
using System.Collections.Generic;
using Enemies.Scripts.Behaviours;
using Player.Scripts;
using PrimeTween;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Tools_and_Scripts;
using UnityEngine;

public enum BiscottoSideSelection
{
    Random,
    Clockwise,
    CounterClockwise
}

[Serializable]
public sealed class BiscottoPunchComboBehaviour : IEnemyBehaviour
{
    private const float DamageColorTransitionDuration = 0.05f;

    [OdinSerialize]
    [LabelText("Nom du pattern")]
    private string patternName = "Grosse Patate";

    [OdinSerialize]
    [Required]
    [LabelText("Prefab de zone rectangulaire")]
    private GameObject rectangularDamageZonePrefab;

    [OdinSerialize]
    [ListDrawerSettings(ShowFoldout = true, DefaultExpandedState = true)]
    [LabelText("Coups")]
    private List<BiscottoPunchStep> punchSteps = new List<BiscottoPunchStep>();

    [OdinSerialize]
    [MinValue(0.001f)]
    [LabelText("Lissage de la visée")]
    private float rotationDampening = 0.08f;

    [OdinSerialize]
    [MinValue(0.0f)]
    [LabelText("Récupération finale")]
    private float finalRecoveryDuration = 0.8f;

    [OdinSerialize]
    [LabelText("After-image pendant le déplacement")]
    private bool triggerAfterImageOnSideMove = true;

    [NonSerialized] private Sequence attackSequence;
    [NonSerialized] private Sequence moveSequence;
    [NonSerialized] private RectangleDamageZone currentDamageZone;
    [NonSerialized] private Transform currentDamageZoneRoot;
    [NonSerialized] private float currentAimEndTimestamp;
    [NonSerialized] private readonly List<RectangleDamageZone> spawnedDamageZones = new List<RectangleDamageZone>();

    public void StartBehaviour(EnemyController enemy, BehaviourExecution execution)
    {
        ResetRuntimeState();

        int stepCount = punchSteps != null ? punchSteps.Count : 0;
        if (stepCount == 0)
        {
            Debug.LogError($"[{enemy.name}] Le pattern '{patternName}' ne contient aucun coup valide.", enemy);
            execution.Complete();
            return;
        }

        if (rectangularDamageZonePrefab == null && !HasAnyStepPrefabOverride())
        {
            Debug.LogError($"[{enemy.name}] Le pattern '{patternName}' nécessite un prefab de zone rectangulaire.", enemy);
            execution.Complete();
            return;
        }

        attackSequence = Sequence.Create();

        foreach (BiscottoPunchStep step in punchSteps)
        {
            if (step == null)
                continue;

            BiscottoPunchStep capturedStep = step;
            attackSequence.Chain(CreatePunchStepSequence(enemy, capturedStep));
        }

        attackSequence
            .ChainDelay(finalRecoveryDuration)
            .ChainCallback(() => execution.Complete());
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
        ResetRuntimeState();
    }

    public void CancelBehaviour(EnemyController enemy)
    {
        ResetRuntimeState();
    }

    public void SetSubBehaviourState(bool state)
    {
    }

    private Sequence CreatePunchStepSequence(EnemyController enemy, BiscottoPunchStep step)
    {
        Sequence sequence = Sequence.Create();

        if (step.DelayBeforeStep > 0.0f)
            sequence.ChainDelay(step.DelayBeforeStep);

        if (step.MoveBesidePlayer)
        {
            sequence
                .ChainCallback(() => StartSideMove(enemy, step))
                .ChainDelay(step.SideMoveDuration);
        }

        sequence.ChainCallback(() =>
        {
            PlayAnimation(enemy, step.AnticipationAnimation);
            SpawnPunchZone(enemy, step);
        });

        float timeToDamageCheck = step.SpawnDuration + step.FillDuration + DamageColorTransitionDuration;
        float impactLeadTime = Mathf.Min(step.ImpactAnimationLeadTime, timeToDamageCheck);
        float timeBeforeImpactAnimation = timeToDamageCheck - impactLeadTime;

        if (timeBeforeImpactAnimation > 0.0f)
            sequence.ChainDelay(timeBeforeImpactAnimation);

        if (!string.IsNullOrWhiteSpace(step.ImpactAnimation))
            sequence.ChainCallback(() => PlayAnimation(enemy, step.ImpactAnimation));

        if (impactLeadTime > 0.0f)
            sequence.ChainDelay(impactLeadTime);

        if (step.DelayAfterImpact > 0.0f)
            sequence.ChainDelay(step.DelayAfterImpact);

        return sequence;
    }

    private void SpawnPunchZone(EnemyController enemy, BiscottoPunchStep step)
    {
        GameObject zonePrefab = step.RectangularDamageZonePrefabOverride != null
            ? step.RectangularDamageZonePrefabOverride
            : rectangularDamageZonePrefab;

        if (zonePrefab == null)
        {
            Debug.LogError($"[{enemy.name}] Aucun prefab de zone n'est configuré pour l'étape '{step.StepName}'.", enemy);
            ClearCurrentAimTarget();
            return;
        }

        GameObject zoneObject = UnityEngine.Object.Instantiate(zonePrefab, enemy.transform.position, Quaternion.identity);
        RectangleDamageZone damageZone = zoneObject.GetComponentInChildren<RectangleDamageZone>();

        if (damageZone == null)
        {
            Debug.LogError($"[{enemy.name}] Le prefab '{zonePrefab.name}' ne contient pas de RectangleDamageZone.", zonePrefab);
            UnityEngine.Object.Destroy(zoneObject);
            ClearCurrentAimTarget();
            return;
        }

        currentDamageZone = damageZone;
        currentDamageZoneRoot = zoneObject.transform;
        currentAimEndTimestamp = Time.time + Mathf.Max(
            0.0f,
            step.SpawnDuration + step.FillDuration + DamageColorTransitionDuration - step.LockBeforeImpact);

        spawnedDamageZones.Add(damageZone);
        RotateCurrentZoneTowardPlayer(enemy, true);
        damageZone.Setup(Vector2.right, step.SpawnDuration, step.FillDuration);
    }

    private void RotateCurrentZoneTowardPlayer(EnemyController enemy, bool immediate = false)
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

        float dampening = Mathf.Max(0.001f, rotationDampening);
        currentDamageZoneRoot.rotation = Quaternion.Slerp(
            currentDamageZoneRoot.rotation,
            targetRotation,
            Time.deltaTime / dampening);
    }

    private void StartSideMove(EnemyController enemy, BiscottoPunchStep step)
    {
        if (PlayerStateMachine.instance == null || step.SideMoveDuration <= 0.0f)
            return;

        if (moveSequence.isAlive)
            moveSequence.Stop();

        Vector3 destination = ComputeSideDestination(enemy, step);

        if (triggerAfterImageOnSideMove && enemy.afterImage != null)
            enemy.afterImage.Trigger(step.SideMoveDuration);

        moveSequence = Sequence.Create()
            .Group(Tween.Position(enemy.transform, destination, step.SideMoveDuration, Ease.InOutCubic));
    }

    private static Vector3 ComputeSideDestination(EnemyController enemy, BiscottoPunchStep step)
    {
        Vector3 playerPosition = PlayerStateMachine.instance.position;
        Vector3 directionToPlayer = playerPosition - enemy.transform.position;
        directionToPlayer.y = 0.0f;

        if (directionToPlayer.sqrMagnitude <= 0.0001f)
            directionToPlayer = enemy.transform.forward;

        directionToPlayer.Normalize();
        Vector3 clockwiseSide = new Vector3(directionToPlayer.z, 0.0f, -directionToPlayer.x);

        float sideSign;
        switch (step.SideSelection)
        {
            case BiscottoSideSelection.Clockwise:
                sideSign = 1.0f;
                break;
            case BiscottoSideSelection.CounterClockwise:
                sideSign = -1.0f;
                break;
            default:
                sideSign = UnityEngine.Random.value < 0.5f ? -1.0f : 1.0f;
                break;
        }

        Vector3 destination = playerPosition + clockwiseSide * sideSign * step.SideMoveDistance;
        destination.y = enemy.transform.position.y;
        return destination;
    }

    private static void PlayAnimation(EnemyController enemy, string animationName)
    {
        if (enemy.animator == null || string.IsNullOrWhiteSpace(animationName))
            return;

        enemy.animator.Play(animationName);
    }


    private bool HasAnyStepPrefabOverride()
    {
        if (punchSteps == null)
            return false;

        foreach (BiscottoPunchStep step in punchSteps)
        {
            if (step != null && step.RectangularDamageZonePrefabOverride != null)
                return true;
        }

        return false;
    }

    private void ClearCurrentAimTarget()
    {
        currentDamageZone = null;
        currentDamageZoneRoot = null;
        currentAimEndTimestamp = 0.0f;
    }

    private void ResetRuntimeState()
    {
        if (attackSequence.isAlive)
            attackSequence.Stop();

        if (moveSequence.isAlive)
            moveSequence.Stop();

        foreach (RectangleDamageZone zone in spawnedDamageZones)
        {
            if (zone != null)
                zone.Cancel();
        }

        attackSequence = default;
        moveSequence = default;
        spawnedDamageZones.Clear();
        ClearCurrentAimTarget();
    }
}
