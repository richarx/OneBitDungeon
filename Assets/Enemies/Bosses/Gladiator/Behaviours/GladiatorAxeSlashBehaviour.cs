using Enemies.Scripts.Behaviours;
using Player.Scripts;
using PrimeTween;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Tools_and_Scripts;
using UnityEngine;

public class GladiatorAxeSlashBehaviour : IEnemyBehaviour
{
    [OdinSerialize]
    [Required]
    [LabelText("Data")]
    private GladiatorAxeSlashData data;

    private Sequence attackSequence;
    private ConeDamageZone currentDamageZone;

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

        Vector3 targetPosition = ComputeTargetMovementPosition(enemy, data.MoveDistance);
        string direction = (targetPosition.x - enemy.transform.position.x) >= 0.0f ? "R" : "L";

        attackSequence = Sequence.Create()
            .ChainCallback(() => PlayAnimation(enemy, $"Dash_{direction}_Axe"))
            .ChainCallback(() =>
            {
                if (data.TriggerAfterImageOnSideMove && enemy.afterImage != null)
                    enemy.afterImage.Trigger(data.MoveDuration);
            })
            .Chain(Tween.Position(enemy.transform, targetPosition, data.MoveDuration, Ease.OutCirc))
            .ChainCallback(() => PlayAnimation(enemy, data.AnticipationAnimation))
            .ChainCallback(() => SpawnDamageZone(enemy))
            .ChainDelay(data.SpawnDuration + data.FillDuration)
            .ChainCallback(() => PlayAnimation(enemy, data.ImpactAnimation))
            .ChainDelay(0.5f)
            .ChainCallback(() => PlayAnimation(enemy, "Idle_NoAxe"))
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

    private void SpawnDamageZone(EnemyController enemy)
    {
        currentDamageZone = GameObject.Instantiate(
            data.ConeDamageZonePrefab,
            enemy.transform.position,
            Quaternion.identity);

        Vector3 directionToPlayer = (PlayerStateMachine.instance.position - enemy.transform.position).normalized;

        currentDamageZone.Setup(
            directionToPlayer.ToVector2(),
            data.Radius,
            data.HalfAngle * 2.0f,
            data.SpawnDuration,
            data.FillDuration);
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

        if (currentDamageZone != null)
            currentDamageZone.Cancel();

        attackSequence = default;
        currentDamageZone = null;
    }

    private static void PlayAnimation(EnemyController enemy, string animationName)
    {
        if (enemy.animator != null && !string.IsNullOrWhiteSpace(animationName))
            enemy.animator.Play(animationName);
    }
}
