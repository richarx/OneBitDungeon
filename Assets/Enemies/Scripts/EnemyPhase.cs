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

    public IEnemyBehaviour GetTransitionBehaviour()
    {
        return transitionBehaviour.GetComponent<IEnemyBehaviour>();
    }

    public List<IEnemyBehaviour> GetBehaviours()
    {
        List<IEnemyBehaviour> enemyBehaviours = new List<IEnemyBehaviour>();

        foreach (GameObject behaviour in phaseBehaviours)
        {
            IEnemyBehaviour behaviourPrefab = behaviour.GetComponent<IEnemyBehaviour>();
            behaviourPrefab.SetSubBehaviourState(false);
            enemyBehaviours.Add(behaviourPrefab);
        }

        return enemyBehaviours;
    }
}
