using Enemies.Scripts.Behaviours;
using PrimeTween;
using UnityEngine;

public class GladiatorSpawn : MonoBehaviour, IEnemyBehaviour
{
    public void StartBehaviour(EnemyController enemy)
    {
        Sequence.Create()
            .ChainDelay(3.0f)
            .ChainCallback(() => enemy.SelectNewBehaviour(true));
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
    }

    public void SetSubBehaviourState(bool state)
    {
    }
}
