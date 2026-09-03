using System.Collections;
using System.Collections.Generic;
using Enemies.Scripts.Behaviours;
using Player.Scripts;
using PrimeTween;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Tools_and_Scripts;
using UnityEngine;

public class GladiatorAxeComboBehaviour : IEnemyBehaviour
{
    [OdinSerialize]
    [Required]
    [LabelText("Data")]
    private GladiatorAxeComboData data;

    private Sequence attackSequence;
    private ConeDamageZone firstDamageZone;
    private ConeDamageZone secondDamageZone;
    private const float DamageColorTransitionDuration = 0.05f;

    public void StartBehaviour(EnemyController enemy, BehaviourExecution execution)
    {
        if (data == null)
        {
            Debug.LogError("[GladiatorAxeSlashBehaviour] Un data est requis.", enemy);
            execution.Complete();
            return;
        }

        if (PlayerStateMachine.instance == null)
        {
            Debug.LogError("[GladiatorAxeSlashBehaviour] Aucun joueur n'est disponible.", enemy);
            execution.Complete();
            return;
        }

        if (PlayerStateMachine.instance.position.z >= 7.5f)
        {
            execution.Complete();
            return;
        }

        Vector3 targetPosition = ComputeTargetMovementPosition(enemy, data.FirstMoveDistance);
        string direction = (targetPosition.x - enemy.transform.position.x) >= 0.0f ? "R" : "L";

        attackSequence = Sequence.Create()
            .ChainCallback(() => PlayAnimation(enemy, $"Dash_{direction}_Axe"))
            .ChainCallback(() =>
            {
                if (data.TriggerAfterImageOnFirstSideMove && enemy.afterImage != null)
                    enemy.afterImage.Trigger(data.FirstMoveDuration);
            })
            .Chain(Tween.Position(enemy.transform, targetPosition, data.FirstMoveDuration, Ease.OutCirc))
            .ChainCallback(() => PlayAnimation(enemy, data.FirstAnticipationAnimation))
            .ChainCallback(() => SpawnFirstDamageZone(enemy))
            .ChainDelay(data.FirstSpawnDuration + data.FirstFillDuration + DamageColorTransitionDuration)
            .ChainCallback(() => PlayAnimation(enemy, data.FirstImpactAnimation))
            .ChainDelay(0.3f)
            .ChainCallback(() => ComputeSecondAttack(enemy, execution));
    }

    private void ComputeSecondAttack(EnemyController enemy, BehaviourExecution execution)
    {
        if (attackSequence.isAlive)
            attackSequence.Stop();

        attackSequence = Sequence.Create();

        if (data.RepositionBetweenAttacks)
        {
            Vector3 targetPosition = ComputeTargetMovementPosition(enemy, data.SecondMoveDistance);
            string direction = (targetPosition.x - enemy.transform.position.x) >= 0.0f ? "R" : "L";

            attackSequence
                .ChainCallback(() =>
                {
                    if (data.TriggerAfterImageOnSecondSideMove && enemy.afterImage != null)
                        enemy.afterImage.Trigger(data.SecondMoveDuration);
                })
                .Chain(Tween.Position(enemy.transform, targetPosition, data.SecondMoveDuration, Ease.OutCirc));
        }

        attackSequence
            .ChainCallback(() => SpawnSecondDamageZone(enemy))
            .ChainDelay(data.SecondSpawnDuration + data.SecondFillDuration + DamageColorTransitionDuration)
            .ChainCallback(() => PlayAnimation(enemy, data.SecondImpactAnimation))
            .ChainDelay(0.5f)
            .ChainCallback(() => execution.Complete());
    }

    private Vector3 ComputeTargetMovementPosition(EnemyController enemy, float moveDistance)
    {
        Vector3 position = PlayerStateMachine.instance.position;

        position.z += moveDistance;

        return ClampPositionInArena(position);
    }

    private Vector3 ClampPositionInArena(Vector3 position)
    {
        position.x = Mathf.Clamp(position.x, -9.0f, 9.0f);
        position.z = Mathf.Clamp(position.z, -9.0f, 9.0f);

        return position;
    }

    private void SpawnFirstDamageZone(EnemyController enemy)
    {
        firstDamageZone = GameObject.Instantiate(
            data.ConeDamageZonePrefab,
            enemy.transform.position,
            Quaternion.identity);

        Vector3 directionToPlayer = (PlayerStateMachine.instance.position - enemy.transform.position).normalized;

        firstDamageZone.Setup(
            directionToPlayer.ToVector2(),
            data.FirstRadius,
            data.FirstHalfAngle * 2.0f,
            data.FirstSpawnDuration,
            data.FirstFillDuration);
    }

    private void SpawnSecondDamageZone(EnemyController enemy)
    {
        secondDamageZone = GameObject.Instantiate(
            data.ConeDamageZonePrefab,
            enemy.transform.position,
            Quaternion.identity);

        Vector3 directionToPlayer = (PlayerStateMachine.instance.position - enemy.transform.position).normalized;

        secondDamageZone.Setup(
            directionToPlayer.ToVector2(),
            data.SecondRadius,
            data.SecondHalfAngle * 2.0f,
            data.SecondSpawnDuration,
            data.SecondFillDuration);
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
        if (attackSequence.isAlive)
            attackSequence.Stop();

        if (firstDamageZone != null)
            firstDamageZone.Cancel();

        if (secondDamageZone != null)
            secondDamageZone.Cancel();

        attackSequence = default;
        firstDamageZone = null;
        secondDamageZone = null;
    }

    private static void PlayAnimation(EnemyController enemy, string animationName)
    {
        if (enemy.animator != null && !string.IsNullOrWhiteSpace(animationName))
            enemy.animator.Play(animationName);
    }
}
