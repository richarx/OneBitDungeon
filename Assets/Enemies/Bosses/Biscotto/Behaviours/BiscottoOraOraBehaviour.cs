using System;
using System.Collections.Generic;
using Enemies.Scripts.Behaviours;
using Player.Scripts;
using PrimeTween;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Tools_and_Scripts;
using UnityEngine;

[Serializable]
public sealed class BiscottoOraOraBehaviour : IEnemyBehaviour
{
    private const float DamageColorTransitionDuration = 0.05f;

    private sealed class ActivePunch
    {
        public RectangleDamageZone DamageZone;
        public Transform Root;
        public float AimEndTimestamp;
    }

    [OdinSerialize]
    [Required]
    [LabelText("Data")]
    private BiscottoOraOraData data;

    private Sequence attackSequence;
    private Sequence moveSequence;
    private List<ActivePunch> activePunches = new List<ActivePunch>();
    private BehaviourExecution currentExecution;
    private int launchedPunchCount;
    private float nextPunchTimestamp;
    private float repositionEndTimestamp;
    private bool isRepositioning;
    private bool isFinishing;

    public void StartBehaviour(EnemyController enemy, BehaviourExecution execution)
    {
        ResetRuntimeState();

        if (data == null)
        {
            Debug.LogError("[BiscottoOraOraBehaviour] Un data Ora Ora est requis.", enemy);
            execution.Complete();
            return;
        }

        if (data.RectangularDamageZonePrefab == null)
        {
            Debug.LogError("[BiscottoOraOraBehaviour] Un prefab de zone rectangulaire est requis.", enemy);
            execution.Complete();
            return;
        }

        if (PlayerStateMachine.instance == null)
        {
            Debug.LogError("[BiscottoOraOraBehaviour] Aucun joueur n'est disponible.", enemy);
            execution.Complete();
            return;
        }

        currentExecution = execution;
        launchedPunchCount = 0;
        nextPunchTimestamp = Time.time;
    }

    public void UpdateBehaviour(EnemyController enemy)
    {
        for (int index = activePunches.Count - 1; index >= 0; index--)
        {
            ActivePunch punch = activePunches[index];
            if (punch == null || punch.DamageZone == null || punch.Root == null)
            {
                activePunches.RemoveAt(index);
                continue;
            }

            if (Time.time < punch.AimEndTimestamp)
                RotatePunchTowardPlayer(punch.Root);
        }

        UpdatePunchBarrage(enemy);
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

    private void LaunchPunch(EnemyController enemy, int punchIndex)
    {
        PlayAnimation(enemy, data.PunchAnimation);

        GameObject zoneObject = UnityEngine.Object.Instantiate(
            data.RectangularDamageZonePrefab,
            enemy.transform.position + ComputeDamageZoneOffset(enemy, punchIndex),
            Quaternion.identity);
        RectangleDamageZone damageZone = zoneObject.GetComponentInChildren<RectangleDamageZone>();

        if (damageZone == null)
        {
            Debug.LogError($"[{enemy.name}] Le prefab '{data.RectangularDamageZonePrefab.name}' ne contient pas de RectangleDamageZone.", data.RectangularDamageZonePrefab);
            UnityEngine.Object.Destroy(zoneObject);
            return;
        }

        ActivePunch punch = new ActivePunch
        {
            DamageZone = damageZone,
            Root = zoneObject.transform,
            AimEndTimestamp = Time.time + Mathf.Max(
                0.0f,
                data.SpawnDuration + data.FillDuration + DamageColorTransitionDuration - data.LockBeforeImpact)
        };

        activePunches.Add(punch);
        damageZone.SetDimensions(data.DamageZoneWidth, data.DamageZoneLength);
        RotatePunchTowardPlayer(punch.Root, true);
        damageZone.Setup(Vector2.right, data.SpawnDuration, data.FillDuration);
    }

    private Vector3 ComputeDamageZoneOffset(EnemyController enemy, int punchIndex)
    {
        if (data.DamageZoneSideOffset <= 0.0f)
            return Vector3.zero;

        Vector3 directionToPlayer = PlayerStateMachine.instance != null
            ? PlayerStateMachine.instance.position - enemy.transform.position
            : enemy.transform.forward;
        directionToPlayer.y = 0.0f;

        if (directionToPlayer.sqrMagnitude <= 0.0001f)
            directionToPlayer = enemy.transform.forward;

        directionToPlayer.Normalize();
        Vector3 rightSide = new Vector3(directionToPlayer.z, 0.0f, -directionToPlayer.x);
        float sideSign = GetPunchSide(punchIndex) == BiscottoPunchSide.Right ? 1.0f : -1.0f;
        return rightSide * sideSign * data.DamageZoneSideOffset;
    }

    private BiscottoPunchSide GetPunchSide(int punchIndex)
    {
        bool isEvenPunch = punchIndex % 2 == 0;
        if (isEvenPunch)
            return data.FirstPunchSide;

        return data.FirstPunchSide == BiscottoPunchSide.Left
            ? BiscottoPunchSide.Right
            : BiscottoPunchSide.Left;
    }

    private void UpdatePunchBarrage(EnemyController enemy)
    {
        if (isFinishing || launchedPunchCount >= data.PunchCount)
            return;

        if (isRepositioning)
        {
            if (Time.time < repositionEndTimestamp)
                return;

            isRepositioning = false;
            LaunchAndScheduleNextPunch(enemy);
            return;
        }

        if (Time.time < nextPunchTimestamp)
            return;

        if (ShouldRepositionForPlayerDistance(enemy))
        {
            StartReposition(enemy);
            isRepositioning = data.RepositionDuration > 0.0f;
            repositionEndTimestamp = Time.time + data.RepositionDuration;

            if (!isRepositioning)
                LaunchAndScheduleNextPunch(enemy);

            return;
        }

        LaunchAndScheduleNextPunch(enemy);
    }

    private bool ShouldRepositionForPlayerDistance(EnemyController enemy)
    {
        if (PlayerStateMachine.instance == null)
            return false;

        Vector3 distanceToPlayer = PlayerStateMachine.instance.position - enemy.transform.position;
        distanceToPlayer.y = 0.0f;
        return distanceToPlayer.sqrMagnitude >= data.RepositionDistanceThreshold * data.RepositionDistanceThreshold;
    }

    private void LaunchAndScheduleNextPunch(EnemyController enemy)
    {
        LaunchPunch(enemy, launchedPunchCount);
        launchedPunchCount++;

        if (launchedPunchCount < data.PunchCount)
        {
            nextPunchTimestamp = Time.time + data.PunchInterval;
            return;
        }

        BeginFinalRecovery(enemy);
    }

    private void BeginFinalRecovery(EnemyController enemy)
    {
        isFinishing = true;
        attackSequence = Sequence.Create()
            .ChainDelay(data.SpawnDuration + data.FillDuration + DamageColorTransitionDuration)
            .ChainCallback(() => PlayAnimation(enemy, data.RecoveryAnimation))
            .ChainDelay(data.FinalRecoveryDuration)
            .ChainCallback(() =>
            {
                if (enemy.IsExecutionActive(currentExecution))
                    currentExecution.Complete();
            });
    }

    private void StartReposition(EnemyController enemy)
    {
        if (PlayerStateMachine.instance == null || data.RepositionDuration <= 0.0f)
            return;

        if (moveSequence.isAlive)
            moveSequence.Stop();

        // Les zones de la position précédente ne doivent pas poursuivre Biscotto.
        CancelActivePunches();

        Vector3 startPosition = enemy.transform.position;
        Vector3 pivotPosition = PlayerStateMachine.instance.position;
        pivotPosition.y = startPosition.y;
        Vector3 destination = ComputeRepositionDestination(enemy);

        if (data.TriggerAfterImageOnReposition && enemy.afterImage != null)
            enemy.afterImage.Trigger(data.RepositionDuration);

        moveSequence = Sequence.Create()
            .Group(Tween.Custom(0.0f, 1.0f, data.RepositionDuration, progress =>
            {
                enemy.transform.position = GetSphericalMovePosition(
                    startPosition,
                    destination,
                    pivotPosition,
                    progress);
            }, Ease.InOutSine));
    }

    private Vector3 ComputeRepositionDestination(EnemyController enemy)
    {
        Vector3 playerPosition = PlayerStateMachine.instance.position;
        Vector3 directionToPlayer = playerPosition - enemy.transform.position;
        directionToPlayer.y = 0.0f;

        if (directionToPlayer.sqrMagnitude <= 0.0001f)
            directionToPlayer = enemy.transform.forward;

        directionToPlayer.Normalize();
        Vector3 lateralDirection = new Vector3(directionToPlayer.z, 0.0f, -directionToPlayer.x);
        float lateralOffset = UnityEngine.Random.Range(
            -data.RepositionLateralRandomness,
            data.RepositionLateralRandomness);

        Vector3 destination = playerPosition
            - directionToPlayer * data.RepositionDistanceToPlayer
            + lateralDirection * lateralOffset;
        destination.y = enemy.transform.position.y;
        return destination;
    }

    private static Vector3 GetSphericalMovePosition(
        Vector3 startPosition,
        Vector3 destination,
        Vector3 pivotPosition,
        float progress)
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

    private void RotatePunchTowardPlayer(Transform punchRoot, bool immediate = false)
    {
        if (punchRoot == null || PlayerStateMachine.instance == null)
            return;

        Vector3 direction = PlayerStateMachine.instance.position - punchRoot.position;
        direction.y = 0.0f;

        if (direction.sqrMagnitude <= 0.0001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(
            direction.normalized.ToVector2().AddAngleToDirection(90.0f).ToVector3());

        if (immediate)
        {
            punchRoot.rotation = targetRotation;
            return;
        }

        punchRoot.rotation = Quaternion.Slerp(
            punchRoot.rotation,
            targetRotation,
            Time.deltaTime / Mathf.Max(0.001f, data.RotationDampening));
    }

    private void ResetRuntimeState()
    {
        if (attackSequence.isAlive)
            attackSequence.Stop();

        if (moveSequence.isAlive)
            moveSequence.Stop();

        CancelActivePunches();
        attackSequence = default;
        moveSequence = default;
        currentExecution = null;
        launchedPunchCount = 0;
        nextPunchTimestamp = 0.0f;
        repositionEndTimestamp = 0.0f;
        isRepositioning = false;
        isFinishing = false;
    }

    private void CancelActivePunches()
    {
        if (activePunches == null)
        {
            activePunches = new List<ActivePunch>();
            return;
        }

        foreach (ActivePunch punch in activePunches)
        {
            if (punch != null && punch.DamageZone != null)
                punch.DamageZone.Cancel();
        }

        activePunches.Clear();
    }

    private static void PlayAnimation(EnemyController enemy, string animationName)
    {
        if (enemy.animator != null && !string.IsNullOrWhiteSpace(animationName))
            enemy.animator.Play(animationName);
    }
}
