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

public enum BiscottoPunchSide
{
    Left,
    Right
}

[Serializable]
public sealed class BiscottoPunchComboBehaviour : IEnemyBehaviour
{
    private const float DamageColorTransitionDuration = 0.05f;

    [OdinSerialize]
    [Required]
    [LabelText("Data")]
    private BiscottoPunchComboData data;

    private Sequence attackSequence;
    private Sequence moveSequence;
    private RectangleDamageZone currentDamageZone;
    private Transform currentDamageZoneRoot;
    private BiscottoPunchStep currentPunchStep;
    private float currentAimEndTimestamp;
    private List<RectangleDamageZone> spawnedDamageZones = new List<RectangleDamageZone>();

    public void StartBehaviour(EnemyController enemy, BehaviourExecution execution)
    {
        ResetRuntimeState();

        if (data == null)
        {
            Debug.LogError("[BiscottoPunchComboBehaviour] Un data de combo est requis.", enemy);
            execution.Complete();
            return;
        }

        int stepCount = data.PunchSteps != null ? data.PunchSteps.Count : 0;
        if (stepCount == 0)
        {
            Debug.LogError($"[{enemy.name}] Le pattern '{data.PatternName}' ne contient aucun coup valide.", enemy);
            execution.Complete();
            return;
        }

        if (data.RectangularDamageZonePrefab == null && !HasAnyStepPrefabOverride())
        {
            Debug.LogError($"[{enemy.name}] Le pattern '{data.PatternName}' nécessite un prefab de zone rectangulaire.", enemy);
            execution.Complete();
            return;
        }

        attackSequence = Sequence.Create();

        foreach (BiscottoPunchStep step in data.PunchSteps)
        {
            if (step == null)
                continue;

            BiscottoPunchStep capturedStep = step;
            attackSequence.Chain(CreatePunchStepSequence(enemy, capturedStep));
        }

        attackSequence
            .ChainDelay(data.FinalRecoveryDuration)
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

        float timeToDamageCheck = step.SpawnDuration + step.FillDuration;

        sequence.ChainDelay(timeToDamageCheck);

        if (!string.IsNullOrWhiteSpace(step.ImpactAnimation))
            sequence.ChainCallback(() => PlayAnimation(enemy, step.ImpactAnimation));

        if (step.DelayAfterImpact > 0.0f)
            sequence.ChainDelay(step.DelayAfterImpact);

        return sequence;
    }

    private void SpawnPunchZone(EnemyController enemy, BiscottoPunchStep step)
    {
        GameObject zonePrefab = step.RectangularDamageZonePrefabOverride != null
            ? step.RectangularDamageZonePrefabOverride
            : data.RectangularDamageZonePrefab;

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
        currentPunchStep = step;
        currentAimEndTimestamp = Time.time + Mathf.Max(
            0.0f,
            step.SpawnDuration + step.FillDuration + DamageColorTransitionDuration - step.LockBeforeImpact);

        spawnedDamageZones.Add(damageZone);
        damageZone.SetDimensions(step.DamageZoneWidth, step.DamageZoneLength);
        RotateCurrentZoneTowardPlayer(enemy, true);
        damageZone.Setup(Vector2.right, step.SpawnDuration, step.FillDuration);
    }

    private static Vector3 ComputeDamageZoneOffset(EnemyController enemy, BiscottoPunchStep step)
    {
        if (step.DamageZoneSideOffset <= 0.0f)
            return Vector3.zero;

        Vector3 directionToPlayer = PlayerStateMachine.instance != null
            ? PlayerStateMachine.instance.position - enemy.transform.position
            : enemy.transform.forward;
        directionToPlayer.y = 0.0f;

        if (directionToPlayer.sqrMagnitude <= 0.0001f)
            directionToPlayer = enemy.transform.forward;

        directionToPlayer.Normalize();
        Vector3 rightSide = new Vector3(directionToPlayer.z, 0.0f, -directionToPlayer.x);
        float sideSign = step.DamageZoneSide == BiscottoPunchSide.Right ? 1.0f : -1.0f;

        return rightSide * sideSign * step.DamageZoneSideOffset;
    }

    private void RotateCurrentZoneTowardPlayer(EnemyController enemy, bool immediate = false)
    {
        if (currentDamageZoneRoot == null || currentPunchStep == null || PlayerStateMachine.instance == null)
            return;

        currentDamageZoneRoot.position = enemy.transform.position + ComputeDamageZoneOffset(enemy, currentPunchStep);

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

    private void StartSideMove(EnemyController enemy, BiscottoPunchStep step)
    {
        if (PlayerStateMachine.instance == null || step.SideMoveDuration <= 0.0f)
            return;

        if (moveSequence.isAlive)
            moveSequence.Stop();

        float sideSign = GetSideSign(step.SideSelection);
        Vector3 startPosition = enemy.transform.position;
        Vector3 pivotPosition = PlayerStateMachine.instance.position;
        pivotPosition.y = startPosition.y;
        Vector3 destination = ComputeSideDestination(enemy, step, sideSign);

        if (data.TriggerAfterImageOnSideMove && enemy.afterImage != null)
            enemy.afterImage.Trigger(step.SideMoveDuration);

        moveSequence = Sequence.Create()
            .Group(Tween.Custom(0.0f, 1.0f, step.SideMoveDuration, progress =>
            {
                enemy.transform.position = GetSideMoveArcPosition(startPosition, destination, pivotPosition, progress);
            }, Ease.InOutSine));
    }

    private static Vector3 ComputeSideDestination(EnemyController enemy, BiscottoPunchStep step, float sideSign)
    {
        Vector3 playerPosition = PlayerStateMachine.instance.position;
        Vector3 directionToPlayer = playerPosition - enemy.transform.position;
        directionToPlayer.y = 0.0f;

        if (directionToPlayer.sqrMagnitude <= 0.0001f)
            directionToPlayer = enemy.transform.forward;

        directionToPlayer.Normalize();
        Vector3 clockwiseSide = new Vector3(directionToPlayer.z, 0.0f, -directionToPlayer.x);

        Vector3 destination = playerPosition + clockwiseSide * sideSign * step.SideMoveDistance;
        destination.y = enemy.transform.position.y;
        return destination;
    }

    private static float GetSideSign(BiscottoSideSelection sideSelection)
    {
        switch (sideSelection)
        {
            case BiscottoSideSelection.Clockwise:
                return 1.0f;
            case BiscottoSideSelection.CounterClockwise:
                return -1.0f;
            default:
                return UnityEngine.Random.value < 0.5f ? -1.0f : 1.0f;
        }
    }

    private static Vector3 GetSideMoveArcPosition(Vector3 startPosition, Vector3 destination, Vector3 pivotPosition, float progress)
    {
        Vector2 startOffset = (startPosition - pivotPosition).ToVector2();
        Vector2 endOffset = (destination - pivotPosition).ToVector2();

        float startRadius = startOffset.magnitude;
        float endRadius = endOffset.magnitude;

        if (startRadius <= 0.0001f || endRadius <= 0.0001f)
            return Vector3.Lerp(startPosition, destination, progress);

        float startAngle = Mathf.Atan2(startOffset.y, startOffset.x) * Mathf.Rad2Deg;
        float endAngle = Mathf.Atan2(endOffset.y, endOffset.x) * Mathf.Rad2Deg;
        float angle = startAngle + Mathf.DeltaAngle(startAngle, endAngle) * progress;
        float radius = Mathf.Lerp(startRadius, endRadius, progress);
        float angleInRadians = angle * Mathf.Deg2Rad;

        return new Vector3(
            pivotPosition.x + Mathf.Cos(angleInRadians) * radius,
            Mathf.Lerp(startPosition.y, destination.y, progress),
            pivotPosition.z + Mathf.Sin(angleInRadians) * radius);
    }

    private static void PlayAnimation(EnemyController enemy, string animationName)
    {
        if (enemy.animator == null || string.IsNullOrWhiteSpace(animationName))
            return;

        enemy.animator.Play(animationName);
    }


    private bool HasAnyStepPrefabOverride()
    {
        if (data == null || data.PunchSteps == null)
            return false;

        foreach (BiscottoPunchStep step in data.PunchSteps)
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
        currentPunchStep = null;
        currentAimEndTimestamp = 0.0f;
    }

    private void ResetRuntimeState()
    {
        if (attackSequence.isAlive)
            attackSequence.Stop();

        if (moveSequence.isAlive)
            moveSequence.Stop();

        if (spawnedDamageZones == null)
        {
            spawnedDamageZones = new List<RectangleDamageZone>();
        }
        else
        {
            foreach (RectangleDamageZone zone in spawnedDamageZones)
            {
                if (zone != null)
                    zone.Cancel();
            }

            spawnedDamageZones.Clear();
        }

        attackSequence = default;
        moveSequence = default;
        ClearCurrentAimTarget();
    }
}
