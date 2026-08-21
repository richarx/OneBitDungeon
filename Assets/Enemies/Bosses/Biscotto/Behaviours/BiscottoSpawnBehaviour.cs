using System;
using Enemies.Scripts.Behaviours;
using Enemies.Spawner;
using PrimeTween;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

[Serializable]
public sealed class BiscottoSpawnBehaviour : IEnemyBehaviour
{
    [OdinSerialize]
    [Required]
    [LabelText("Data")]
    private BiscottoSpawnData data;

    [NonSerialized] private Sequence spawnSequence;

    public void StartBehaviour(EnemyController enemy, BehaviourExecution execution)
    {
        CancelSequence();

        if (data == null)
        {
            UnityEngine.Debug.LogError("[BiscottoSpawnBehaviour] Un data d'apparition est requis.", enemy);
            execution.Complete();
            return;
        }

        EnemyHolder.instance.RegisterEnemy(enemy.gameObject);

        spawnSequence = Sequence.Create()
            .ChainDelay(data.SpawnDelay)
            .ChainCallback(() => execution.Complete());
    }

    public void UpdateBehaviour(EnemyController enemy)
    {
    }

    public void FixedUpdateBehaviour(EnemyController enemy)
    {
    }

    public void StopBehaviour(EnemyController enemy)
    {
    }

    public void CancelBehaviour(EnemyController enemy)
    {
        CancelSequence();
    }

    public void SetSubBehaviourState(bool state)
    {
    }

    private void CancelSequence()
    {
        if (spawnSequence.isAlive)
            spawnSequence.Stop();
    }
}
