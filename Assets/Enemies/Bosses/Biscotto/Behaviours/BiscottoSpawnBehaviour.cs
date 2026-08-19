using System;
using Enemies.Scripts.Behaviours;
using Enemies.Spawner;
using PrimeTween;

[Serializable]
public sealed class BiscottoSpawnBehaviour : IEnemyBehaviour
{
    [NonSerialized] private Sequence spawnSequence;

    public void StartBehaviour(EnemyController enemy, BehaviourExecution execution)
    {
        CancelSequence();
        EnemyHolder.instance.RegisterEnemy(enemy.gameObject);

        spawnSequence = Sequence.Create()
            .ChainDelay(3.0f)
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
