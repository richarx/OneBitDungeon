using System;
using Enemies.Scripts.Behaviours;
using PrimeTween;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Tools_and_Scripts;
using UnityEngine;

public class GladiatorSkippingRopeBehaviour : IEnemyBehaviour
{
    [OdinSerialize]
    [Required]
    [LabelText("Data")]
    private GladiatorSkippingRopeData data;

    [NonSerialized] private Sequence attackSequence;
    [NonSerialized] private SkippingRopeController ropeController;

    private bool isSpinning;

    public void StartBehaviour(EnemyController enemy, BehaviourExecution execution)
    {
        Vector3 targetPosition = Vector3.zero;
        string direction = (targetPosition.x - enemy.transform.position.x) >= 0.0f ? "R" : "L";

        attackSequence = Sequence.Create()
            .ChainCallback(() => PlayAnimation(enemy, $"Dash_{direction}_Axe"))
            .ChainCallback(() =>
            {
                if (data.TriggerAfterImageOnSideMove && enemy.afterImage != null)
                    enemy.afterImage.Trigger(data.MoveDuration);
            })
            .Chain(Tween.Position(enemy.transform, Vector3.zero, data.MoveDuration))
            .ChainCallback(() => PlayAnimation(enemy, data.AnticipationAnimation))
            .ChainDelay(data.AnticipationAnimationDuration)
            .ChainCallback(() => PlayAnimation(enemy, data.RopeThrowAnimation))
            .ChainDelay(data.RopeThrowAnimationDuration)
            .ChainCallback(() => SendHookAxe(enemy))
            .ChainCallback(() => isSpinning = true)
            .ChainDelay(data.SkippingDuration - data.RetractionDuration)
            .ChainCallback(() => ropeController.Retract(data.RetractionDuration))
            .ChainDelay(data.RetractionDuration)
            .ChainCallback(() => isSpinning = false)
            .ChainCallback(() => PlayAnimation(enemy, data.RecoveryAnimation))
            .ChainDelay(data.RecoveryAnimationDuration)
            .ChainCallback(() => execution.Complete());
    }

    private void SendHookAxe(EnemyController enemy)
    {
        ropeController = UnityEngine.Object.Instantiate(data.ropeControllerPrefab, enemy.transform.position + data.ThrowOffset, Quaternion.identity);
        ropeController.Setup(data.FlyDistance, data.ExtensionDuration);
    }

    public void UpdateBehaviour(EnemyController enemy)
    {
        if (isSpinning)
            RotateRope();
    }

    private void RotateRope()
    {
        ropeController.transform.rotation = Quaternion.Slerp(
            ropeController.transform.rotation,
            Quaternion.LookRotation(ropeController.transform.right.ToVector2().AddAngleToDirection(data.RotationSpeed).ToVector3()),
            Time.deltaTime / data.RotationDampening
        );
    }

    public void FixedUpdateBehaviour(EnemyController enemy)
    {
    }

    public void CancelBehaviour(EnemyController enemy)
    {
        ResetRuntimeState();
    }

    public void StopBehaviour(EnemyController enemy)
    {
        ResetRuntimeState();
    }

    public void SetSubBehaviourState(bool state)
    {
    }

    private void ResetRuntimeState()
    {
        if (attackSequence.isAlive)
            attackSequence.Stop();

        if (ropeController != null)
            ropeController.Retract(0.5f);

        isSpinning = false;
        attackSequence = default;
        ropeController = null;
    }

    private static void PlayAnimation(EnemyController enemy, string animationName)
    {
        if (enemy.animator != null && !string.IsNullOrWhiteSpace(animationName))
            enemy.animator.Play(animationName);
    }
}
