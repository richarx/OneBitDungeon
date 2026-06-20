using System.Collections;
using System.Collections.Generic;
using Enemies.Scripts.Behaviours;
using UnityEngine;

public class DummyCircleAttack : MonoBehaviour, IEnemyBehaviour
{
    [SerializeField] private CircleDamageZone circleDamageZonePrefab;
    [SerializeField] private float attackInterval = 5.0f;
    [SerializeField] private float radius = 2.5f;
    [SerializeField] private float spawnDuration = 0.3f;
    [SerializeField] private float fillDuration = 0.8f;

    private float nextAttackTime;
    private readonly List<CircleDamageZone> activeZones = new List<CircleDamageZone>();

    public void StartBehaviour(EnemyController enemy)
    {
        nextAttackTime = Time.time + attackInterval;
    }

    public void UpdateBehaviour(EnemyController enemy)
    {
        if (Time.time < nextAttackTime)
            return;

        SpawnCircleAttack(enemy);
        nextAttackTime = Time.time + attackInterval;
    }

    private void SpawnCircleAttack(EnemyController enemy)
    {
        CircleDamageZone zone = Instantiate(
            circleDamageZonePrefab,
            enemy.transform.position,
            Quaternion.Euler(90.0f, 0.0f, 0.0f)
        );

        zone.Setup(radius, spawnDuration, fillDuration);
        activeZones.Add(zone);
    }

    public void FixedUpdateBehaviour(EnemyController enemy)
    {
    }

    public void StopBehaviour(EnemyController enemy)
    {
    }

    public void CancelBehaviour(EnemyController enemy)
    {
        foreach (CircleDamageZone zone in activeZones)
        {
            if (zone != null)
                zone.Cancel();
        }

        activeZones.Clear();
    }

    public void SetSubBehaviourState(bool state)
    {
    }
}
