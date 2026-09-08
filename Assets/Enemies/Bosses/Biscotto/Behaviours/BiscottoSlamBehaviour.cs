using System;
using Enemies.Scripts.Behaviours;
using Player.Scripts;
using PrimeTween;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

public class BiscottoSlamBehaviour : IEnemyBehaviour
{
    [OdinSerialize]
    [Required]
    [LabelText("Data")]
    private BiscottoSlamData data;

    private Sequence attackSequence;
    private CircleDamageZone currentDamageZone;

    private const float DamageFlashDuration = 0.05f;

    public void StartBehaviour(EnemyController enemy, BehaviourExecution execution)
    {
        ResetRuntimeState(enemy);

        if (data == null)
        {
            Debug.LogError("[BiscottoTsarBombaBehaviour] Un data Tsar Bomba est requis.", enemy);
            execution.Complete();
            return;
        }

        if (data.CircleDamageZonePrefab == null)
        {
            Debug.LogError("[BiscottoTsarBombaBehaviour] Un prefab de zone circulaire est requis.", enemy);
            execution.Complete();
            return;
        }

        if (PlayerStateMachine.instance == null)
        {
            Debug.LogError("[BiscottoTsarBombaBehaviour] Aucun joueur n'est disponible.", enemy);
            execution.Complete();
            return;
        }

        if (data.IsChainingBehaviour)
            enemy.EnqueueBehaviour(data.chainedBehaviour);

        float timeToDamage = data.SpawnDuration + data.FillDuration;

        attackSequence = Sequence.Create();

        if (data.MoveToArenaCenter)
            attackSequence = ComputeMovement(enemy, attackSequence);

        attackSequence
            .ChainCallback(() => PlayAnimation(enemy, data.AnticipationAnimation))
            .ChainCallback(() => SpawnDamageZone(enemy))
            .ChainDelay(timeToDamage)
            .ChainCallback(() => PlayAnimation(enemy, data.ImpactAnimation))
            .ChainDelay(DamageFlashDuration)
            .ChainDelay(data.RecoveryDuration)
            .ChainCallback(() => execution.Complete());
    }

    private void SpawnDamageZone(EnemyController enemy)
    {
        currentDamageZone = UnityEngine.Object.Instantiate(
            data.CircleDamageZonePrefab,
            enemy.transform.position,
            Quaternion.Euler(90.0f, 0.0f, 0.0f));

        currentDamageZone.Setup(data.Radius, data.SpawnDuration, data.FillDuration);
    }

    private Sequence ComputeMovement(EnemyController enemy, Sequence attackSequence)
    {
        Vector3 targetPosition = Vector3.zero;

        attackSequence
            .ChainCallback(() => PlayAnimation(enemy, "Idle"))
            .ChainCallback(() =>
            {
                if (data.TriggerAfterImageOnSideMove && enemy.afterImage != null)
                    enemy.afterImage.Trigger(data.MoveDuration);
            })
            .Chain(Tween.Position(enemy.transform, targetPosition, data.MoveDuration, Ease.OutCirc));

        return attackSequence;
    }

    public void UpdateBehaviour(EnemyController enemy)
    {
    }

    public void FixedUpdateBehaviour(EnemyController enemy)
    {
    }

    public void CancelBehaviour(EnemyController enemy)
    {
        ResetRuntimeState(enemy);
    }

    public void StopBehaviour(EnemyController enemy)
    {
        ResetRuntimeState(enemy);
    }

    public void SetSubBehaviourState(bool state)
    {
    }

    private void ResetRuntimeState(EnemyController enemy)
    {
        if (attackSequence.isAlive)
            attackSequence.Stop();

        if (currentDamageZone != null && !currentDamageZone.IsDestroyed)
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
