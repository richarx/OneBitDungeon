using System;
using Enemies.Scripts.Behaviours;
using Player.Scripts;
using PrimeTween;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Tools_and_Scripts;
using UnityEngine;

[Serializable]
public sealed class GladiatorThrowAxeBehaviour : IEnemyBehaviour
{
    [OdinSerialize, Required, LabelText("Données Gladiateur")]
    private GladiatorData gladiatorData;

    [OdinSerialize, Required, LabelText("Prefab zone rectangulaire")]
    private GameObject rectangleDamageZonePrefab;

    [OdinSerialize, Required, LabelText("Prefab hache")]
    private AxeController axePrefab;

    [NonSerialized] private Sequence attackSequence;
    [NonSerialized] private RectangleDamageZone rectangleDamageZone;
    [NonSerialized] private float startAimingTimestamp;
    [NonSerialized] private Vector3 rotationDirection;

    public GladiatorThrowAxeBehaviour()
    {
    }

    public GladiatorThrowAxeBehaviour(GladiatorData gladiatorData, GameObject rectangleDamageZonePrefab, AxeController axePrefab)
    {
        this.gladiatorData = gladiatorData;
        this.rectangleDamageZonePrefab = rectangleDamageZonePrefab;
        this.axePrefab = axePrefab;
    }

    public void StartBehaviour(EnemyController enemy, BehaviourExecution execution)
    {
        ResetRuntimeState();

        Vector3 randomPosition = new Vector3(UnityEngine.Random.Range(-7.0f, 7.0f), 0.0f, UnityEngine.Random.Range(6.0f, 8.0f));
        string direction = (randomPosition.x - enemy.transform.position.x) >= 0.0f ? "R" : "L";

        attackSequence = Sequence.Create()
            .ChainCallback(() => enemy.animator.Play($"Dash_{direction}_Axe"))
            .Chain(MoveToPosition(enemy, randomPosition, gladiatorData.throwMoveDuration))
            .ChainCallback(() => enemy.animator.Play("ThrowAxe_Anticipation"))
            .ChainCallback(() => SpawnRectangleZone(enemy))
            .ChainDelay(gladiatorData.throwSpawnDuration + gladiatorData.throwFillDuration - gladiatorData.throwAnimationDuration)
            .ChainCallback(() => enemy.animator.Play("ThrowAxe"))
            .ChainDelay(gladiatorData.throwAnimationDuration)
            .ChainCallback(() => SpawnAxe(enemy, execution));
    }

    public void UpdateBehaviour(EnemyController enemy)
    {
        if (rectangleDamageZone != null && Time.time - startAimingTimestamp <= gladiatorData.throwRotationDuration)
            rotationDirection = RotateThrowTowardPlayer();
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

    private void SpawnRectangleZone(EnemyController enemy)
    {
        startAimingTimestamp = Time.time;
        GameObject rectangle = UnityEngine.Object.Instantiate(rectangleDamageZonePrefab, enemy.transform.position, Quaternion.identity);
        rectangleDamageZone = rectangle.GetComponentInChildren<RectangleDamageZone>();
        if (rectangleDamageZone == null)
        {
            Debug.LogError("[GladiatorThrowAxeBehaviour] The rectangle prefab has no RectangleDamageZone.", enemy);
            UnityEngine.Object.Destroy(rectangle);
            return;
        }

        rectangleDamageZone.Setup(Vector2.right, gladiatorData.throwSpawnDuration, gladiatorData.throwFillDuration);
    }

    private void SpawnAxe(EnemyController enemy, BehaviourExecution execution)
    {
        AxeController axe = UnityEngine.Object.Instantiate(axePrefab, enemy.transform.position, Quaternion.identity);
        axe.Setup(rotationDirection, gladiatorData.throwAxeDistance, gladiatorData.throwAxeFlyDuration, () => CatchAxe(enemy, execution));
    }

    private void CatchAxe(EnemyController enemy, BehaviourExecution execution)
    {
        if (!enemy.IsExecutionActive(execution))
            return;

        attackSequence = Sequence.Create()
            .ChainCallback(() => enemy.animator.Play("CatchAxe"))
            .ChainDelay(0.3f)
            .ChainCallback(() => execution.Complete());
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

    private Vector3 RotateThrowTowardPlayer()
    {
        Vector3 position = rectangleDamageZone.transform.parent.position;
        Vector3 direction = (PlayerStateMachine.instance.position - position).normalized;

        rectangleDamageZone.transform.parent.rotation = Quaternion.Slerp(
            rectangleDamageZone.transform.parent.rotation,
            Quaternion.LookRotation(direction.ToVector2().AddAngleToDirection(90.0f).ToVector3()),
            Time.deltaTime / gladiatorData.throwRotationDampening
        );

        return direction;
    }

    private void ResetRuntimeState()
    {
        if (attackSequence.isAlive)
            attackSequence.Stop();

        if (rectangleDamageZone != null)
            rectangleDamageZone.Cancel();

        attackSequence = default;
        rectangleDamageZone = null;
        startAimingTimestamp = 0.0f;
        rotationDirection = Vector3.zero;
    }
}
