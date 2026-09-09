using System;
using Enemies.Scripts.Behaviours;
using Player.Scripts;
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
    [NonSerialized] private float startRotationTimestamp;

    [NonSerialized] private ArroganceProcessor _arroganceProcessor;

    private bool isSpinning;

    public void StartBehaviour(EnemyController enemy, BehaviourExecution execution)
    {
        Vector3 targetPosition = Vector3.zero;
        string direction = (targetPosition.x - enemy.transform.position.x) >= 0.0f ? "R" : "L";

        _arroganceProcessor = PlayerStateMachine.instance.GetComponent<ArroganceProcessor>();
        if (_arroganceProcessor == null)
        {
            Debug.LogWarning("[GladiatorSkippingRopeBehaviour] No arrogance processor on PlayerStateMachine");
        }


        attackSequence = Sequence.Create()
            .ChainCallback(() => PlayAnimation(enemy, $"Dash_{direction}_Axe"))
            .ChainCallback(() =>
            {
                if (data.TriggerAfterImageOnSideMove && enemy.afterImage != null)
                    enemy.afterImage.Trigger(data.MoveDuration);
            })
            .Chain(EnemyMovementUtility.CreateMoveToPosition(enemy, Vector3.zero, data.MoveDuration))
            .ChainCallback(() => PlayAnimation(enemy, data.AnticipationAnimation))
            .ChainDelay(data.AnticipationAnimationDuration)
            .ChainCallback(() => PlayAnimation(enemy, data.RopeThrowAnimation))
            .ChainDelay(data.RopeThrowAnimationDuration)
            .ChainCallback(() => SendHookAxe(enemy))
            .ChainCallback(() => startRotationTimestamp = Time.time)
            .ChainCallback(() => isSpinning = true)
            .ChainDelay(data.ExtensionDuration - 2)
            .ChainCallback(() => { if (_arroganceProcessor != null) _arroganceProcessor.TauntGainMultiplier = 4; })
            .ChainDelay(data.SkippingDuration - data.RetractionDuration - data.ExtensionDuration +2 )
            .ChainCallback(() => { if (_arroganceProcessor != null) _arroganceProcessor.TauntGainMultiplier = 1; })
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
        float normalizedTime = Tools.NormalizeValue(Time.time - startRotationTimestamp, 0.0f, data.SkippingDuration);
        float rotationSpeed = data.RotationAccelerationCurve.Evaluate(normalizedTime) * data.RotationSpeed;

        ropeController.transform.rotation = Quaternion.Slerp(
            ropeController.transform.rotation,
            Quaternion.LookRotation(ropeController.transform.right.ToVector2().AddAngleToDirection(rotationSpeed).ToVector3()),
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
        if (_arroganceProcessor != null)
            _arroganceProcessor.TauntGainMultiplier = 1;
        if (attackSequence.isAlive)
            attackSequence.Stop();

        if (ropeController != null)
            ropeController.Retract(0.5f);

        startRotationTimestamp = 0.0f;
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
