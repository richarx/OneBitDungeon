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
    [OdinSerialize, Required, LabelText("Données Gladiateur")]
    private GladiatorData gladiatorData;

    [OdinSerialize, Required, LabelText("Prefab zone circulaire")]
    private CircleDamageZone circleDamageZonePrefab;

    [OdinSerialize, Required, LabelText("Prefab piège")]
    private TrapController trapControllerPrefab;

    [NonSerialized] private Sequence attackSequence;
    [NonSerialized] private List<CircleDamageZone> circles;

    public GladiatorTrapsBehaviour()
    {
    }

    public GladiatorTrapsBehaviour(GladiatorData gladiatorData, CircleDamageZone circleDamageZonePrefab, TrapController trapControllerPrefab)
    {
        this.gladiatorData = gladiatorData;
        this.circleDamageZonePrefab = circleDamageZonePrefab;
        this.trapControllerPrefab = trapControllerPrefab;
    }

    public void StartBehaviour(EnemyController enemy, BehaviourExecution execution)
    {
        ResetRuntimeState();

        Vector3 randomPosition = new Vector3(UnityEngine.Random.Range(-7.0f, 7.0f), 0.0f, UnityEngine.Random.Range(-7.0f, 5.0f));
        string direction = (randomPosition.x - enemy.transform.position.x) >= 0.0f ? "R" : "L";

        attackSequence = Sequence.Create()
            .ChainCallback(() => enemy.animator.Play($"Dash_{direction}_Axe"))
            .Chain(MoveToPosition(enemy, randomPosition, gladiatorData.trapsMoveDuration))
            .ChainCallback(() => enemy.animator.Play("ThrowTraps"))
            .ChainCallback(SpawnCircleZones)
            .ChainDelay(gladiatorData.trapsAnimationDuration)
            .ChainCallback(() => SpawnTraps(enemy))
            .ChainDelay(0.5f)
            .ChainCallback(() => execution.Complete());
    }

    public void UpdateBehaviour(EnemyController enemy)
    {
    }

    public void FixedUpdateBehaviour(EnemyController enemy)
    {
    }

    public void StopBehaviour(EnemyController enemy)
    {
    }

    public void CancelBehaviour(EnemyController enemy)
    {
        ResetRuntimeState();
    }

    public void SetSubBehaviourState(bool state)
    {
    }

    private Sequence MoveToPosition(EnemyController enemy, Vector3 enemyPosition, float moveDuration)
    {
        bool isSecondPhase = enemy.currentPhase > 0;

        return Sequence.Create()
            .ChainCallback(() =>
            {
                if (isSecondPhase)
                    enemy.afterImage.Trigger(moveDuration);
            })
            .Group(Tween.Position(enemy.transform, enemyPosition, moveDuration, Ease.InOutCubic));
    }

    private void SpawnCircleZones()
    {
        circles = new List<CircleDamageZone>();

        Vector3 playerPosition = PlayerStateMachine.instance.position;
        Vector2 direction = UnityEngine.Random.insideUnitCircle.normalized;

        for (int i = 0; i < 3; i++)
        {
            Vector3 position = playerPosition + direction.ToVector3() * gladiatorData.trapsDistanceFromPlayer;
            CircleDamageZone circle = UnityEngine.Object.Instantiate(circleDamageZonePrefab, position, Quaternion.Euler(90.0f, 0.0f, 0.0f));
            circle.Setup(gladiatorData.trapsZoneRadius, gladiatorData.trapsSpawnDuration, gladiatorData.trapsFillDuration);
            circles.Add(circle);

            direction = direction.AddAngleToDirection(120.0f);
        }
    }

    private void SpawnTraps(EnemyController enemy)
    {
        foreach (CircleDamageZone circle in circles)
        {
            if (circle == null)
                continue;

            TrapController trap = UnityEngine.Object.Instantiate(trapControllerPrefab, enemy.transform.position, Quaternion.identity);
            trap.Setup(circle.transform.position, gladiatorData.trapsFlyDuration, gladiatorData.trapsStartingHeight);
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
}
