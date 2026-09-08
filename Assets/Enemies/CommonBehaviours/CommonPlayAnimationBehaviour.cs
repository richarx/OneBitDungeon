using System;
using System.Collections;
using System.Collections.Generic;
using Enemies.Scripts.Behaviours;
using PrimeTween;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

public class CommonPlayAnimationBehaviour : IEnemyBehaviour
{
    [OdinSerialize]
    [Required]
    [LabelText("Data")]
    private CommonPlayAnimationData data;

    [NonSerialized] private Sequence animationSequence;
    [NonSerialized] private int healthPointsAtStart;
    [NonSerialized] private BehaviourExecution currentExecution;

    public void StartBehaviour(EnemyController enemy, BehaviourExecution execution)
    {
        ResetRuntimeState();

        healthPointsAtStart = enemy.damageable.currentHealth;
        currentExecution = execution;

        if (data.IsChainingAnimation)
            enemy.EnqueueBehaviour(data.chainedBehaviour);

        animationSequence = Sequence.Create();

        animationSequence = ComputeMovement(enemy, animationSequence);

        animationSequence
            .ChainCallback(() => PlayAnimation(enemy, data.Animation))
            .ChainDelay(data.AnimationDuration)
            .ChainCallback(() => execution.Complete());
    }

    private Sequence ComputeMovement(EnemyController enemy, Sequence sequence)
    {
        if (data.MoveToArenaCenter)
        {
            Vector3 targetPosition = Vector3.zero;

            sequence
                .ChainCallback(() => PlayAnimation(enemy, data.MoveAnimation))
                .ChainCallback(() =>
                {
                    if (data.TriggerAfterImageOnSideMove && enemy.afterImage != null)
                        enemy.afterImage.Trigger(data.MoveDuration);
                })
                .Chain(Tween.Position(enemy.transform, targetPosition, data.MoveDuration, Ease.OutCirc));
        }

        return sequence;
    }

    public void UpdateBehaviour(EnemyController enemy)
    {
        if (data.DamageThreshold > 0 && healthPointsAtStart - enemy.damageable.currentHealth >= data.DamageThreshold)
        {
            currentExecution.Complete();
            ResetRuntimeState();
        }
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
        if (animationSequence.isAlive)
            animationSequence.Stop();

        animationSequence = default;
        currentExecution = null;
    }

    private static void PlayAnimation(EnemyController enemy, string animationName)
    {
        if (enemy.animator != null && !string.IsNullOrWhiteSpace(animationName))
            enemy.animator.Play(animationName);
    }
}
