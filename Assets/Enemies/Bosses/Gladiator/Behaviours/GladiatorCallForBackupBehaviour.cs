using System;
using Enemies.Scripts.Behaviours;
using PrimeTween;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Tools_and_Scripts;
using UnityEngine;

public class GladiatorCallForBackupBehaviour : IEnemyBehaviour
{
    [OdinSerialize]
    [Required]
    [LabelText("Data")]
    private GladiatorCallForBackupData data;

    [NonSerialized] private Sequence animationSequence;

    public void StartBehaviour(EnemyController enemy, BehaviourExecution execution)
    {
        ResetRuntimeState();

        animationSequence = Sequence.Create();

        animationSequence = ComputeMovement(enemy, animationSequence);

        animationSequence
            .ChainCallback(() => PlayAnimation(enemy, data.AnticipationAnimation))
            .ChainDelay(data.DelayBeforeSpawn);

        for (int i = 0; i < data.CorumCount; i++)
        {
            animationSequence
                .ChainCallback(() => SpawnCorum())
                .ChainDelay(data.TimeBeetweenSpawns);
        }

        animationSequence
            .ChainDelay(data.DelayAfterSpawn)
            .ChainCallback(() => PlayAnimation(enemy, "Idle"))
            .ChainCallback(() => execution.Complete());
    }

    private Sequence ComputeMovement(EnemyController enemy, Sequence sequence)
    {
        if (data.IsMovingToBackOfArena)
        {
            Vector3 targetPosition = new Vector3(0.0f, 0.0f, 7.0f);
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

    private void SpawnCorum()
    {
        Vector3 position = new Vector3(Tools.RandomPositiveOrNegative(UnityEngine.Random.Range(3.0f, 9.0f)), 0.0f, 10.0f);
        GameObject.Instantiate(data.CorumPrefab, position, Quaternion.identity);
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

    private void ResetRuntimeState()
    {
        if (animationSequence.isAlive)
            animationSequence.Stop();

        animationSequence = default;
    }

    private static void PlayAnimation(EnemyController enemy, string animationName)
    {
        if (enemy.animator != null && !string.IsNullOrWhiteSpace(animationName))
            enemy.animator.Play(animationName);
    }
}
