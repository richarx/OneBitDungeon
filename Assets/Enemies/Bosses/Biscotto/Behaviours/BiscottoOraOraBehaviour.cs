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

        // OraOra commence toujours après un repositionnement vers le joueur.
        if (data.RepositionDuration > 0.0f)
        {
            StartReposition(enemy);
            isRepositioning = true;
            repositionEndTimestamp = Time.time + data.RepositionDuration;
        }
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
                data.SpawnDuration + data.FillDuration - data.LockBeforeImpact)
        };

        activePunches.Add(punch);
        //SetPunchRenderingOrder(damageZone, punchIndex);
        damageZone.SetDimensions(data.DamageZoneWidth, data.DamageZoneLength);
        RotatePunchTowardPlayer(punch.Root, true);
        damageZone.Setup(Vector2.right, data.SpawnDuration, data.FillDuration);
    }

    private static void SetPunchRenderingOrder(RectangleDamageZone damageZone, int punchIndex)
    {
        SpriteRenderer spriteRenderer = damageZone.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            return;

        // Les premières zones restent visuellement au-dessus des suivantes.
        spriteRenderer.sortingOrder -= punchIndex;
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
            .ChainDelay(data.SpawnDuration + data.FillDuration)
            .ChainCallback(() => PlayAnimation(enemy, data.RecoveryAnimation))
            .ChainDelay(data.FinalRecoveryDuration)
            .ChainCallback(() => PlayAnimation(enemy, "Idle"))
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

        Vector3 pivotPosition = PlayerStateMachine.instance.position;
        pivotPosition.y = enemy.transform.position.y;
        Vector3 destination = ComputeRepositionDestination(enemy);

        if (data.TriggerAfterImageOnReposition && enemy.afterImage != null)
            enemy.afterImage.Trigger(data.RepositionDuration);

        moveSequence = BiscottoMovementUtility.CreateArcMove(
            enemy.transform,
            destination,
            pivotPosition,
            data.RepositionDuration);
    }

    private Vector3 ComputeRepositionDestination(EnemyController enemy)
    {
        Vector3 playerPosition = PlayerStateMachine.instance.position;

        Vector3 destination = playerPosition
            + Vector3.forward * data.RepositionDistanceToPlayer;
        destination.y = enemy.transform.position.y;
        return destination;
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
