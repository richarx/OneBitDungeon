using System.Collections;
using System.Collections.Generic;
using Enemies.Scripts.Behaviours;
using UnityEngine;

public class DummyTransition : MonoBehaviour, IEnemyBehaviour
{
    [SerializeField] private float radius;

    [Space]
    [SerializeField] private CircleDamageZone circleDamageZonePrefab;

    public void FixedUpdateBehaviour(EnemyController enemy)
    {
    }

    public void StartBehaviour(EnemyController enemy)
    {
        enemy.SelectNewBehaviour(true);
    }

    public void StopBehaviour(EnemyController enemy)
    {
    }

    public void UpdateBehaviour(EnemyController enemy)
    {
    }

    public bool isSubBehaviour;
    public void SetSubBehaviourState(bool state)
    {
        isSubBehaviour = state;
    }

    public void CancelBehaviour(EnemyController enemy)
    {

    }
}

