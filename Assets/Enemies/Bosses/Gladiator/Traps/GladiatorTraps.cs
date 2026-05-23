using System.Collections.Generic;
using Enemies.Scripts.Behaviours;
using Player.Scripts;
using PrimeTween;
using Tools_and_Scripts;
using UnityEngine;

public class GladiatorTraps : MonoBehaviour, IEnemyBehaviour
{
    [SerializeField] private GladiatorData gladiatorData;
    [SerializeField] private CircleDamageZone circleDamageZonePrefab;
    [SerializeField] private TrapController trapControllerPrefab;

    private Sequence attackSequence;
    private List<CircleDamageZone> circles;

    public void StartBehaviour(EnemyController enemy)
    {
        Vector3 randomPosition = new Vector3(Random.Range(-7.0f, 7.0f), 0.0f, Random.Range(-7.0f, 5.0f));
        string direction = (randomPosition.x - enemy.transform.position.x) >= 0.0f ? "R" : "L";

        attackSequence = Sequence.Create()
            .ChainCallback(() => enemy.animator.Play($"Dash_{direction}_Axe"))
            .Chain(MoveToPosition(enemy, randomPosition, gladiatorData.trapsMoveDuration))
            .ChainCallback(() => enemy.animator.Play("ThrowTraps"))
            .ChainCallback(() => SpawnCircleZones())
            .ChainDelay(gladiatorData.trapsAnimationDuration)
            .ChainCallback(() => SpawnTraps(enemy))
            .ChainDelay(0.5f)
            .ChainCallback(() => enemy.SelectNewBehaviour());
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
        Vector2 direction = Random.insideUnitCircle.normalized;

        for (int i = 0; i < 3; i++)
        {
            Vector3 position = playerPosition + direction.ToVector3() * gladiatorData.trapsDistanceFromPlayer;
            CircleDamageZone circle = Instantiate(circleDamageZonePrefab, position, Quaternion.Euler(new Vector3(90.0f, 0.0f, 0.0f)));
            circle.Setup(gladiatorData.trapsZoneRadius, gladiatorData.trapsSpawnDuration, gladiatorData.trapsFillDuration);
            circles.Add(circle);

            direction = direction.AddAngleToDirection(120.0f);
        }
    }

    private void SpawnTraps(EnemyController enemy)
    {
        Vector3 position = enemy.transform.position;

        foreach (CircleDamageZone circle in circles)
        {
            TrapController trap = Instantiate(trapControllerPrefab, position, Quaternion.identity);
            trap.Setup(circle.transform.position, gladiatorData.trapsFlyDuration, gladiatorData.trapsStartingHeight);
        }
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
        if (attackSequence.isAlive)
            attackSequence.Stop();

        foreach (CircleDamageZone circle in circles)
        {
            circle.Cancel();
        }
    }

    public void SetSubBehaviourState(bool state)
    {
    }
}
