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
public sealed class GladiatorTrapsBehaviour : IEnemyBehaviour
{
    [OdinSerialize]
    [Required]
    [LabelText("Data")]
    private GladiatorTrapData data;

    [NonSerialized] private Sequence attackSequence;
    [NonSerialized] private List<CircleDamageZone> circles;

    public void StartBehaviour(EnemyController enemy, BehaviourExecution execution)
    {
        ResetRuntimeState();

        Vector3 randomPosition = new Vector3(UnityEngine.Random.Range(-7.0f, 7.0f), 0.0f, UnityEngine.Random.Range(-7.0f, 5.0f));
        string direction = (randomPosition.x - enemy.transform.position.x) >= 0.0f ? "R" : "L";

        attackSequence = Sequence.Create();

        attackSequence = ComputeMovement(enemy, attackSequence);

        attackSequence
            .ChainCallback(() => PlayAnimation(enemy, "ThrowTraps"))
            .ChainCallback(() => SpawnCircleZones(enemy))
            .ChainDelay(data.ThrowAnimationDuration)
            .ChainCallback(() => SpawnTraps(enemy))
            .ChainDelay(0.5f)
            .ChainCallback(() => execution.Complete());
    }

    private Sequence ComputeMovement(EnemyController enemy, Sequence sequence)
    {
        if (data.MoveAwayFromPlayer || data.MoveToCenterOfArena)
        {
            Vector3 targetPosition = ComputeTargetMovementPosition(enemy, data.MoveDistance);
            string direction = (targetPosition.x - enemy.transform.position.x) >= 0.0f ? "R" : "L";

            sequence
                .ChainCallback(() => PlayAnimation(enemy, $"Dash_{direction}_Axe"))
                .ChainCallback(() =>
                {
                    if (data.TriggerAfterImageOnSideMove && enemy.afterImage != null)
                        enemy.afterImage.Trigger(data.MoveDuration);
                })
                .Chain(Tween.Position(enemy.transform, targetPosition, data.MoveDuration, Ease.OutCirc));
        }

        return sequence;
    }

    private Vector3 ComputeTargetMovementPosition(EnemyController enemy, float moveDistance)
    {
        if (data.MoveToCenterOfArena)
            return Vector3.zero;

        Vector3 currentPosition = enemy.transform.position;
        Vector3 playerPosition = PlayerStateMachine.instance.position;

        Vector3 movementDirection = (currentPosition - playerPosition).normalized;
        Vector3 targetPosition = currentPosition + movementDirection * moveDistance;

        return ClampPositionInArena(targetPosition);
    }

    private Vector3 ClampPositionInArena(Vector3 position)
    {
        position.x = Mathf.Clamp(position.x, -9.0f, 9.0f);
        position.z = Mathf.Clamp(position.z, -9.0f, 9.0f);

        return position;
    }

    public void UpdateBehaviour(EnemyController enemy)
    {
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

    private void SpawnCircleZones(EnemyController enemy)
    {
        circles = new List<CircleDamageZone>();

        if (data.ShootTrapsInCircle)
            SpawnTrapsInCircle(enemy);
        else if (data.ShootTrapsInLine)
            SpawnTrapsInLine(enemy);
    }

    private void SpawnTrapsInCircle(EnemyController enemy)
    {
        Vector3 spawnPosition = Vector3.zero;

        switch (data.CircleTrapTarget)
        {
            case GladiatorTrapData.GladiatorTrapTarget.AroundPlayer:
                spawnPosition = PlayerStateMachine.instance.position;
                break;
            case GladiatorTrapData.GladiatorTrapTarget.AroundBoss:
                spawnPosition = enemy.transform.position;
                break;
            default:
            case GladiatorTrapData.GladiatorTrapTarget.Random:
                spawnPosition = new Vector3(UnityEngine.Random.Range(-5.0f, 5.0f), 0.0f, UnityEngine.Random.Range(-5.0f, 5.0f));
                break;
        }

        Vector2 direction = UnityEngine.Random.insideUnitCircle.normalized;

        float angle = 360.0f / data.TrapCount;

        for (int i = 0; i < data.TrapCount; i++)
        {
            Vector3 position = spawnPosition + direction.ToVector3() * data.TrapsDistanceCenterOfTrapCircle;
            position = ClampPositionInArena(position);
            CircleDamageZone circle = UnityEngine.Object.Instantiate(data.CircleDamageZonePrefab, position, Quaternion.Euler(90.0f, 0.0f, 0.0f));
            circle.Setup(data.ZoneRadius, data.SpawnDuration, data.FillDuration);
            circles.Add(circle);

            direction = direction.AddAngleToDirection(angle);
        }
    }

    private void SpawnTrapsInLine(EnemyController enemy)
    {
        Vector3 startingPosition = enemy.transform.position;
        Vector3 direction = (PlayerStateMachine.instance.position - startingPosition).normalized;
        startingPosition += direction * data.DistanceBetweenTraps;

        for (int i = 0; i < data.TrapCount; i++)
        {
            Vector3 position = startingPosition + direction * data.DistanceBetweenTraps * i;
            position = ClampPositionInArena(position);
            CircleDamageZone circle = UnityEngine.Object.Instantiate(data.CircleDamageZonePrefab, position, Quaternion.Euler(90.0f, 0.0f, 0.0f));
            circle.Setup(data.ZoneRadius, data.SpawnDuration, data.FillDuration);
            circles.Add(circle);
        }
    }

    private void SpawnTraps(EnemyController enemy)
    {
        foreach (CircleDamageZone circle in circles)
        {
            if (circle == null)
                continue;

            TrapController trap = UnityEngine.Object.Instantiate(data.TrapPrefab, enemy.transform.position, Quaternion.identity);
            trap.Setup(circle.transform.position, data.FlyDuration, data.FlyStartingHeight);
        }
    }

    private void ResetRuntimeState()
    {
        if (attackSequence.isAlive)
            attackSequence.Stop();

        if (circles != null)
        {
            foreach (CircleDamageZone circle in circles)
            {
                if (circle != null)
                    circle.Cancel();
            }

            circles.Clear();
        }

        attackSequence = default;
        circles = null;
    }

    private static void PlayAnimation(EnemyController enemy, string animationName)
    {
        if (enemy.animator != null && !string.IsNullOrWhiteSpace(animationName))
            enemy.animator.Play(animationName);
    }
}
