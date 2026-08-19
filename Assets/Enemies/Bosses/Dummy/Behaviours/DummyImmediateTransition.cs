using System;
using System.Collections.Generic;
using Enemies.Scripts.Behaviours;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

[Serializable]
public sealed class DummyImmediateTransitionBehaviour : IEnemyBehaviour
{
    public void StartBehaviour(EnemyController enemy, BehaviourExecution execution)
    {
        execution.Complete();
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
