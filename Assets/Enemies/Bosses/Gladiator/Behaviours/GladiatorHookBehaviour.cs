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
    [OdinSerialize]
    [Required]
    [LabelText("Data")]
    private GladiatorHookData data;

    [NonSerialized] private Sequence attackSequence;
    [NonSerialized] private RectangleDamageZone rectangleDamageZone;
    [NonSerialized] private Transform currentDamageZoneRoot;
    [NonSerialized] private Vector3 rotationDirection;
    [NonSerialized] private float currentAimEndTimestamp;
    private const float DamageColorTransitionDuration = 0.05f;

    public void StartBehaviour(EnemyController enemy, BehaviourExecution execution)
    {
        ResetRuntimeState();

        attackSequence = Sequence.Create();

        attackSequence = ComputeMovement(enemy, attackSequence);

        attackSequence
            .ChainCallback(() => PlayAnimation(enemy, data.AnticipationAnimation))
            .ChainCallback(() => SpawnRectangleZone(enemy))
            .ChainDelay(data.SpawnDuration + data.FillDuration - data.HookThrowAnimationDuration)
            .ChainCallback(() => PlayAnimation(enemy, data.ImpactAnimation))
            .ChainDelay(data.HookThrowAnimationDuration)
            .ChainCallback(() => SendHook(enemy))
            .ChainDelay(data.FlyDuration * 2.0f)
            .ChainCallback(() => PlayAnimation(enemy, "Idle"))
            .ChainCallback(() => execution.Complete());
    }

    private Sequence ComputeMovement(EnemyController enemy, Sequence sequence)
    {
        if (data.MoveAwayFromPlayer)
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
        else if (data.MoveToCornerPosition)
        {
            Vector3 targetPosition = Vector3.zero;
            if (data.GoToOppositeCorner)
                targetPosition = new Vector3(enemy.transform.position.x <= 0.0f ? 6.0f : -6.0f, 0.0f, 6.0f);
            else
                targetPosition = new Vector3(Tools.RandomBool() ? 6.0f : -6.0f, 0.0f, 6.0f);
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
        if (currentDamageZoneRoot != null && Time.time <= currentAimEndTimestamp)
            rotationDirection = RotateThrowTowardPlayer();
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

    private void SpawnRectangleZone(EnemyController enemy)
    {
        GameObject rectangle = UnityEngine.Object.Instantiate(data.RectangleDamageZonePrefab, enemy.transform.position, Quaternion.identity);
        currentDamageZoneRoot = rectangle.transform;

        rectangleDamageZone = rectangle.GetComponentInChildren<RectangleDamageZone>();
        if (rectangleDamageZone == null)
        {
            Debug.LogError("[GladiatorHookBehaviour] The rectangle prefab has no RectangleDamageZone.", enemy);
            UnityEngine.Object.Destroy(rectangle);
            return;
        }

        currentAimEndTimestamp = Time.time + Mathf.Max(
            0.0f,
            data.SpawnDuration + data.FillDuration + DamageColorTransitionDuration - data.LockBeforeImpact);

        rectangleDamageZone.Setup(Vector2.right, data.SpawnDuration, data.FillDuration);
    }

    private void SendHook(EnemyController enemy)
    {
        HookController hook = UnityEngine.Object.Instantiate(data.HookControllerPrefab, enemy.transform.position, Quaternion.identity);
        hook.Setup(rotationDirection, data.FlyDistance, data.FlyDuration, data.PullDistance, data.PullDuration);
    }

    private Vector3 RotateThrowTowardPlayer()
    {
        Vector3 position = currentDamageZoneRoot.position;
        Vector3 direction = (PlayerStateMachine.instance.position - position).normalized;

        currentDamageZoneRoot.rotation = Quaternion.Slerp(
            currentDamageZoneRoot.rotation,
            Quaternion.LookRotation(direction.ToVector2().AddAngleToDirection(90.0f).ToVector3()),
            Time.deltaTime / data.RotationDampening
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
        currentDamageZoneRoot = null;
        currentAimEndTimestamp = 0.0f;
        rotationDirection = Vector3.zero;
    }

    private static void PlayAnimation(EnemyController enemy, string animationName)
    {
        if (enemy.animator != null && !string.IsNullOrWhiteSpace(animationName))
            enemy.animator.Play(animationName);
    }
}
