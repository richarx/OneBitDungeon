using System;
using System.Collections.Generic;
using Enemies.Scripts.Behaviours;
using Player.Scripts;
using PrimeTween;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Tools_and_Scripts;
using UnityEngine;

public enum BiscottoPunchSide
{
    Left,
    Right
}

[Serializable]
public class BiscottoPunchComboBehaviour : IEnemyBehaviour
{
    private const float DamageColorTransitionDuration = 0.05f;
    private const float WallClearance = 0.05f;
    private const int BackdashDirectionSampleCount = 13;
    private const float BackdashMaximumAngle = 90.0f;
    private const float BackdashComparisonTolerance = 0.001f;

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
    private bool _hasHitPlayer;
    private SphereCollider _enemyCollider;

    public void StartBehaviour(EnemyController enemy, BehaviourExecution execution)
    {
        ResetRuntimeState();
        _enemyCollider = enemy.GetComponent<SphereCollider>();

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

        if (data.Backdash)
        {
            attackSequence
                .ChainCallback(() => StartBackdash(enemy))
                .ChainDelay(data.BackdashDuration);
        }

        attackSequence
            .ChainCallback(() => PlayHitAnimationIfNeeded(enemy))
            .ChainDelay(data.FinalRecoveryDuration)
            .ChainCallback(() => PlayAnimation(enemy, "Idle"))
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
                .ChainDelay(step.MoveDuration);
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
        damageZone.OnPlayerHit += HandlePlayerHit;
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
        if (PlayerStateMachine.instance == null || step.MoveDuration <= 0.0f)
            return;

        if (moveSequence.isAlive)
            moveSequence.Stop();

        Vector3 pivotPosition = PlayerStateMachine.instance.position;
        pivotPosition.y = enemy.transform.position.y;
        Vector3 destination = BiscottoMovementUtility.ComputeDestination(
            enemy.transform,
            PlayerStateMachine.instance.position,
            step.MoveDistance
            );

        if (data.TriggerAfterImageOnSideMove && enemy.afterImage != null)
            enemy.afterImage.Trigger(step.MoveDuration);

        moveSequence = BiscottoMovementUtility.CreateArcMove(
            enemy,
            destination,
            pivotPosition,
            step.MoveDuration);
    }

    private void StartBackdash(EnemyController enemy)
    {
        if (PlayerStateMachine.instance == null)
            return;

        if (moveSequence.isAlive)
            moveSequence.Stop();

        Vector3 destination = ComputeBackdashDestination(enemy);
        moveSequence = BiscottoMovementUtility.CreateLinearMove(
            enemy,
            destination,
            data.BackdashDuration);
    }

    private Vector3 ComputeBackdashDestination(EnemyController enemy)
    {
        Vector3 startPosition = enemy.transform.position;
        Vector3 playerPosition = PlayerStateMachine.instance.position;
        Vector3 playerToEnemy = startPosition - playerPosition;
        playerToEnemy.y = 0.0f;
        float currentDistanceToPlayer = playerToEnemy.magnitude;
        float targetDistanceToPlayer = Mathf.Max(currentDistanceToPlayer, data.BackdashDistance);

        if (targetDistanceToPlayer - currentDistanceToPlayer <= BackdashComparisonTolerance)
            return startPosition;

        Vector3 directionAwayFromPlayer = playerToEnemy;
        if (directionAwayFromPlayer.sqrMagnitude <= 0.0001f)
        {
            directionAwayFromPlayer = -enemy.transform.forward;
            directionAwayFromPlayer.y = 0.0f;
        }

        if (directionAwayFromPlayer.sqrMagnitude <= 0.0001f)
            directionAwayFromPlayer = Vector3.back;

        directionAwayFromPlayer.Normalize();

        Vector3 bestDestination = startPosition;
        float bestDistanceToPlayer = currentDistanceToPlayer;
        float bestDistanceToCenter = new Vector2(startPosition.x, startPosition.z).magnitude;
        float bestAwayAlignment = -1.0f;
        bool bestCandidateReachesTarget = false;
        Vector3 directionTowardCenter = new Vector3(-startPosition.x, 0.0f, -startPosition.z);
        bool hasDirectionTowardCenter = directionTowardCenter.sqrMagnitude > 0.0001f;
        float directionTowardCenterAngle = hasDirectionTowardCenter
            ? Mathf.Clamp(
                Vector3.SignedAngle(directionAwayFromPlayer, directionTowardCenter.normalized, Vector3.up),
                -BackdashMaximumAngle,
                BackdashMaximumAngle)
            : 0.0f;
        int directionCount = BackdashDirectionSampleCount + (hasDirectionTowardCenter ? 1 : 0);

        for (int sampleIndex = 0; sampleIndex < directionCount; sampleIndex++)
        {
            float sampleAngle;
            if (sampleIndex < BackdashDirectionSampleCount)
            {
                float sampleProgress = sampleIndex / (BackdashDirectionSampleCount - 1.0f);
                sampleAngle = Mathf.Lerp(-BackdashMaximumAngle, BackdashMaximumAngle, sampleProgress);
            }
            else
            {
                sampleAngle = directionTowardCenterAngle;
            }

            Vector3 candidateDirection = Quaternion.AngleAxis(sampleAngle, Vector3.up) * directionAwayFromPlayer;
            float desiredTravelDistance = ComputeTravelDistanceToTarget(
                playerToEnemy,
                candidateDirection,
                targetDistanceToPlayer);
            float allowedTravelDistance = ComputeWallSafeTravelDistance(
                enemy.transform,
                candidateDirection,
                desiredTravelDistance,
                PlayerStateMachine.instance.obstaclesLayer);

            Vector3 candidateDestination = startPosition + candidateDirection * allowedTravelDistance;
            Vector3 candidateOffsetFromPlayer = candidateDestination - playerPosition;
            candidateOffsetFromPlayer.y = 0.0f;
            float candidateDistanceToPlayer = candidateOffsetFromPlayer.magnitude;
            float candidateDistanceToCenter = new Vector2(candidateDestination.x, candidateDestination.z).magnitude;
            float candidateAwayAlignment = Vector3.Dot(directionAwayFromPlayer, candidateDirection);
            bool candidateReachesTarget = allowedTravelDistance >= desiredTravelDistance - BackdashComparisonTolerance;

            if (!IsBetterBackdashCandidate(
                    candidateReachesTarget,
                    candidateDistanceToPlayer,
                    candidateDistanceToCenter,
                    candidateAwayAlignment,
                    bestCandidateReachesTarget,
                    bestDistanceToPlayer,
                    bestDistanceToCenter,
                    bestAwayAlignment))
            {
                continue;
            }

            bestDestination = candidateDestination;
            bestDistanceToPlayer = candidateDistanceToPlayer;
            bestDistanceToCenter = candidateDistanceToCenter;
            bestAwayAlignment = candidateAwayAlignment;
            bestCandidateReachesTarget = candidateReachesTarget;
        }

        return bestDestination;
    }

    private static float ComputeTravelDistanceToTarget(
        Vector3 playerToEnemy,
        Vector3 movementDirection,
        float targetDistance)
    {
        float projectedDistance = Vector3.Dot(playerToEnemy, movementDirection);
        float remainingSquaredDistance = targetDistance * targetDistance - playerToEnemy.sqrMagnitude;
        float discriminant = projectedDistance * projectedDistance + remainingSquaredDistance;
        return Mathf.Max(0.0f, -projectedDistance + Mathf.Sqrt(Mathf.Max(0.0f, discriminant)));
    }

    private static bool IsBetterBackdashCandidate(
        bool candidateReachesTarget,
        float candidateDistanceToPlayer,
        float candidateDistanceToCenter,
        float candidateAwayAlignment,
        bool bestCandidateReachesTarget,
        float bestDistanceToPlayer,
        float bestDistanceToCenter,
        float bestAwayAlignment)
    {
        if (candidateReachesTarget != bestCandidateReachesTarget)
            return candidateReachesTarget;

        if (candidateReachesTarget)
        {
            if (candidateDistanceToCenter < bestDistanceToCenter - BackdashComparisonTolerance)
                return true;

            return Mathf.Abs(candidateDistanceToCenter - bestDistanceToCenter) <= BackdashComparisonTolerance
                   && candidateAwayAlignment > bestAwayAlignment + BackdashComparisonTolerance;
        }

        if (candidateDistanceToPlayer > bestDistanceToPlayer + BackdashComparisonTolerance)
            return true;

        if (Mathf.Abs(candidateDistanceToPlayer - bestDistanceToPlayer) > BackdashComparisonTolerance)
            return false;

        if (candidateDistanceToCenter < bestDistanceToCenter - BackdashComparisonTolerance)
            return true;

        return Mathf.Abs(candidateDistanceToCenter - bestDistanceToCenter) <= BackdashComparisonTolerance
               && candidateAwayAlignment > bestAwayAlignment + BackdashComparisonTolerance;
    }

    private float ComputeWallSafeTravelDistance(
        Transform enemyTransform,
        Vector3 direction,
        float desiredDistance,
        LayerMask obstacleLayers)
    {
        if (desiredDistance <= 0.0f)
            return 0.0f;

        Vector3 castOrigin = enemyTransform.position;
        float castRadius = 0.0f;

        if (_enemyCollider != null)
        {
            castOrigin = enemyTransform.TransformPoint(_enemyCollider.center);
            Vector3 lossyScale = enemyTransform.lossyScale;
            float largestScale = Mathf.Max(Mathf.Abs(lossyScale.x), Mathf.Abs(lossyScale.y), Mathf.Abs(lossyScale.z));
            castRadius = _enemyCollider.radius * largestScale;
        }

        bool hitWall = castRadius > 0.0f
            ? Physics.SphereCast(
                castOrigin,
                castRadius,
                direction,
                out RaycastHit hit,
                desiredDistance,
                obstacleLayers,
                QueryTriggerInteraction.Ignore)
            : Physics.Raycast(
                castOrigin,
                direction,
                out hit,
                desiredDistance,
                obstacleLayers,
                QueryTriggerInteraction.Ignore);

        return hitWall
            ? Mathf.Max(0.0f, hit.distance - WallClearance)
            : desiredDistance;
    }

    private static void PlayAnimation(EnemyController enemy, string animationName)
    {
        if (enemy.animator == null || string.IsNullOrWhiteSpace(animationName))
            return;

        enemy.animator.Play(animationName);
    }

    private void HandlePlayerHit()
    {
        _hasHitPlayer = true;
    }

    private void PlayHitAnimationIfNeeded(EnemyController enemy)
    {
        if (!_hasHitPlayer || data.HitAnimations == null)
        {
            PlayAnimation(enemy, "Idle");
            return;
        }

        string selectedAnimation = "Idle";
        int validAnimationCount = 0;

        foreach (string animationName in data.HitAnimations)
        {
            if (string.IsNullOrWhiteSpace(animationName))
                continue;

            validAnimationCount++;
            if (UnityEngine.Random.Range(0, validAnimationCount) == 0)
                selectedAnimation = animationName;
        }

        PlayAnimation(enemy, selectedAnimation);
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
                {
                    zone.OnPlayerHit -= HandlePlayerHit;
                    zone.Cancel();
                }
            }

            spawnedDamageZones.Clear();
        }

        attackSequence = default;
        moveSequence = default;
        _hasHitPlayer = false;
        _enemyCollider = null;
        ClearCurrentAimTarget();
    }
}
