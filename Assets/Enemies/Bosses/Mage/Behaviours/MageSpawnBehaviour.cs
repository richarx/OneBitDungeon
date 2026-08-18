using System;
using Enemies.Scripts.Behaviours;
using PrimeTween;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

[Serializable]
public sealed class MageSpawnBehaviour : IEnemyBehaviour
{
    [OdinSerialize] private float radius;
    [OdinSerialize, Required] private CircleDamageZone circleDamageZonePrefab;

    [NonSerialized] private Sequence blastSequence;
    [NonSerialized] private Sequence spawnSequence;
    [NonSerialized] private CircleDamageZone circleDamageZone;
    public MageSpawnBehaviour()
    {
    }

    public MageSpawnBehaviour(float radius, CircleDamageZone circleDamageZonePrefab)
    {
        this.radius = radius;
        this.circleDamageZonePrefab = circleDamageZonePrefab;
    }

    public void StartBehaviour(EnemyController enemy, BehaviourExecution execution)
    {
        ResetRuntimeState();
        enemy.Sprite.transform.position = Vector3.up * 30.0f;
        enemy.shadowSprite.transform.localScale = Vector3.zero;
        enemy.DeactivateHitbox();
        enemy.animator.Play("Ball");
        blastSequence = Sequence.Create()
            .ChainDelay(7.45f)
            .ChainCallback(() =>
            {
                if (enemy.IsExecutionActive(execution))
                    enemy.animator.Play("Blast");
            });

        spawnSequence = Sequence.Create()
            .ChainDelay(2.0f)
            .ChainCallback(() => SpawnDamageZone(enemy.transform.position))
            .ChainDelay(5.0f)
            .Chain(Tween.Alpha(enemy.shadowSprite, 1.0f, 1.0f))
            .Group(Tween.Scale(enemy.shadowSprite.transform, new Vector3(0.1f, 0.1f, 1.0f), Vector3.one, 0.5f))
            .Group(Tween.LocalPositionY(enemy.Sprite.transform, 30.0f, 0.0f, 0.5f, Ease.OutBounce))
            .ChainCallback(() => enemy.ActivateHitbox())
            .ChainDelay(0.5f)
            .ChainCallback(() => execution.Complete());
    }
    public void UpdateBehaviour(EnemyController enemy) { }
    public void FixedUpdateBehaviour(EnemyController enemy) { }
    public void StopBehaviour(EnemyController enemy) { }
    public void CancelBehaviour(EnemyController enemy)
    {
        ResetRuntimeState();
    }

    public void SetSubBehaviourState(bool state) { }

    private void SpawnDamageZone(Vector3 position)
    {
        circleDamageZone = UnityEngine.Object.Instantiate(
            circleDamageZonePrefab,
            position,
            Quaternion.Euler(90.0f, 0.0f, 0.0f));
        circleDamageZone.Setup(radius, 3.0f, 2.5f);
    }

    private void ResetRuntimeState()
    {
        if (blastSequence.isAlive)
            blastSequence.Stop();

        if (spawnSequence.isAlive)
            spawnSequence.Stop();

        blastSequence = default;
        spawnSequence = default;
        circleDamageZone = null;
    }
}
