using System;
using System.Collections.Generic;
using Enemies.Scripts.Behaviours;
using UnityEngine;

[Serializable]
public class EnemyPhase
{
    public int healthThresholdToTriggerTransition;
    public GameObject transitionBehaviour;
    public List<GameObject> phaseBehaviours;

    public IEnemyBehaviour GetTransitionBehaviour(EnemyController enemy)
    {
        if (enemy == null)
        {
            Debug.LogError("[EnemyPhase] Cannot resolve a transition behaviour without an EnemyController.");
            return null;
        }

        return enemy.ResolveBehaviour(transitionBehaviour);
    }

    public List<IEnemyBehaviour> GetBehaviours(EnemyController enemy)
    {
        List<IEnemyBehaviour> enemyBehaviours = new List<IEnemyBehaviour>();

        if (enemy == null)
        {
            Debug.LogError("[EnemyPhase] Cannot resolve attack behaviours without an EnemyController.");
            return enemyBehaviours;
        }

        if (phaseBehaviours == null)
        {
            Debug.LogWarning("[EnemyPhase] No attack behaviours are configured.");
            return enemyBehaviours;
        }

        foreach (GameObject behaviour in phaseBehaviours)
        {
            IEnemyBehaviour runtimeBehaviour = enemy.ResolveBehaviour(behaviour);
            if (runtimeBehaviour == null)
                continue;

            runtimeBehaviour.SetSubBehaviourState(false);
            enemyBehaviours.Add(runtimeBehaviour);
        }

        return enemyBehaviours;
    }
}
