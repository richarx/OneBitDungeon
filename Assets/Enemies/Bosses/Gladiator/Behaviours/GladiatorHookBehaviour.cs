using System;
using Enemies.Scripts.Behaviours;
using Player.Scripts;
using PrimeTween;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Tools_and_Scripts;
using UnityEngine;

[Serializable]
public sealed class GladiatorHookBehaviour : IEnemyBehaviour
{
    [OdinSerialize, Required, LabelText("Données Gladiateur")]
    private GladiatorData gladiatorData;

    [OdinSerialize, Required, LabelText("Prefab zone rectangulaire")]
    private GameObject rectangleDamageZonePrefab;

    [OdinSerialize, Required, LabelText("Prefab grappin")]
    private HookController hookControllerPrefab;

    [NonSerialized] private Sequence attackSequence;
    [NonSerialized] private RectangleDamageZone rectangleDamageZone;
    [NonSerialized] private float startAimingTimestamp;
    [NonSerialized] private Vector3 rotationDirection;

    public GladiatorHookBehaviour()
    {
    }

    public GladiatorHookBehaviour(GladiatorData gladiatorData, GameObject rectangleDamageZonePrefab, HookController hookControllerPrefab)
    {
        this.gladiatorData = gladiatorData;
        this.rectangleDamageZonePrefab = rectangleDamageZonePrefab;
        this.hookControllerPrefab = hookControllerPrefab;
    }

    public void StartBehaviour(EnemyController enemy, BehaviourExecution execution)
    {
        ResetRuntimeState();

        Vector3 randomPosition = new Vector3((Tools.RandomBool() ? 6.0f : -6.0f), 0.0f, 6.0f);
        string direction = (randomPosition.x - enemy.transform.position.x) >= 0.0f ? "R" : "L";

        attackSequence = Sequence.Create()
            .ChainCallback(() => enemy.animator.Play($"Dash_{direction}_Axe"))
            .Chain(MoveToPosition(enemy, randomPosition, gladiatorData.hookMoveDuration))
            .ChainCallback(() => enemy.animator.Play("HookAnticipation"))
            .ChainCallback(() => SpawnRectangleZone(enemy))
            .ChainDelay(gladiatorData.hookSpawnDuration + gladiatorData.hookFillDuration - gladiatorData.hookAnimationDuration)
            .ChainCallback(() => enemy.animator.Play("HookThrow"))
            .ChainDelay(gladiatorData.hookAnimationDuration)
            .ChainCallback(() => SendHook(enemy))
            .ChainDelay(gladiatorData.hookFlyDuration * 2.0f)
            .ChainCallback(() => execution.Complete());
    }

    public void UpdateBehaviour(EnemyController enemy)
    {
        if (rectangleDamageZone != null && Time.time - startAimingTimestamp <= gladiatorData.hookRotationDuration)
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
            Debug.LogError("[GladiatorHookBehaviour] The rectangle prefab has no RectangleDamageZone.", enemy);
            UnityEngine.Object.Destroy(rectangle);
            return;
        }

        rectangleDamageZone.Setup(Vector2.right, gladiatorData.hookSpawnDuration, gladiatorData.hookFillDuration);
    }

    private void SendHook(EnemyController enemy)
    {
        HookController hook = UnityEngine.Object.Instantiate(hookControllerPrefab, enemy.transform.position, Quaternion.identity);
        hook.Setup(rotationDirection, gladiatorData.hookFlyDistance, gladiatorData.hookFlyDuration, gladiatorData.hookPullDistance, gladiatorData.hookPullDuration);
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
