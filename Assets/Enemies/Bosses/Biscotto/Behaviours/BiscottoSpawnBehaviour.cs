using System;
using Enemies.Scripts.Behaviours;
using PrimeTween;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

[Serializable]
public sealed class BiscottoSpawnBehaviour : IEnemyBehaviour
{
    [OdinSerialize]
    [Required]
    [LabelText("Data")]
    private BiscottoSpawnData data;

    [NonSerialized] private Sequence spawnSequence;
    [NonSerialized] private CircleDamageZone currentDamageZone;

    private const float jumpHeight = 15.0f;

    public void StartBehaviour(EnemyController enemy, BehaviourExecution execution)
    {
        ResetRuntimeState(enemy);

        if (data == null)
        {
            UnityEngine.Debug.LogError("[BiscottoSpawnBehaviour] Un data d'apparition est requis.", enemy);
            execution.Complete();
            return;
        }

        Rigidbody rb = enemy.GetComponent<Rigidbody>();

        rb.MovePosition(Vector3.up * jumpHeight);

        spawnSequence = Sequence.Create()
            .ChainDelay(data.WaitDuration)
            .ChainCallback(() => SpawnDamageZone())
            .ChainDelay(data.SpawnDuration + data.FillDuration)
            .ChainCallback(() => PlayAnimation(enemy, data.JumpAnimation))
            .Chain(Tween.RigidbodyMovePosition(rb, Vector3.zero, data.FallDuration))
            .ChainCallback(() => PlayAnimation(enemy, data.ImpactAnimation))
            .ChainCallback(() => RestoreEnemyHitbox(enemy))
            .ChainDelay(data.RecoveryDuration)
            .ChainCallback(() => PlayAnimation(enemy, data.LaughAnimation))
            .ChainDelay(data.LaughDuration)
            .ChainCallback(() => PlayAnimation(enemy, "Idle"))
            .ChainCallback(() => execution.Complete());
    }

    private void SpawnDamageZone()
    {
        currentDamageZone = UnityEngine.Object.Instantiate(
            data.CircleDamageZonePrefab,
            Vector3.zero,
            Quaternion.Euler(90.0f, 0.0f, 0.0f));

        currentDamageZone.Setup(data.Radius, data.SpawnDuration, data.FillDuration);
    }

    public void UpdateBehaviour(EnemyController enemy)
    {
    }

    public void FixedUpdateBehaviour(EnemyController enemy)
    {
    }

    public void StopBehaviour(EnemyController enemy)
    {
        ResetRuntimeState(enemy);
    }

    public void CancelBehaviour(EnemyController enemy)
    {
        ResetRuntimeState(enemy);
    }

    public void SetSubBehaviourState(bool state)
    {
    }

    private void ResetRuntimeState(EnemyController enemy)
    {
        if (spawnSequence.isAlive)
            spawnSequence.Stop();

        Vector3 position = enemy.transform.position;
        position.y = 0.0f;
        enemy.transform.position = position;

        if (currentDamageZone != null && !currentDamageZone.IsDestroyed)
            currentDamageZone.Cancel();

        RestoreEnemyHitbox(enemy);

        spawnSequence = default;
    }

    private void RestoreEnemyHitbox(EnemyController enemy)
    {
        if (enemy.damageable == null || !enemy.damageable.IsDead)
            enemy.ActivateHitbox();
    }

    private static void PlayAnimation(EnemyController enemy, string animationName)
    {
        if (enemy.animator != null && !string.IsNullOrWhiteSpace(animationName))
            enemy.animator.Play(animationName);
    }
}
