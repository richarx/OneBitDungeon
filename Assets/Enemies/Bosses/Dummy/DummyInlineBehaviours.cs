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

[Serializable]
public sealed class DummyCircleAttackBehaviour : IEnemyBehaviour
{
    [OdinSerialize]
    [Required]
    [LabelText("Prefab de zone circulaire")]
    private CircleDamageZone circleDamageZonePrefab;

    [OdinSerialize]
    [MinValue(0.0f)]
    [LabelText("Intervalle d'attaque")]
    private float attackInterval = 4.0f;

    [OdinSerialize]
    [MinValue(0.0f)]
    [LabelText("Rayon")]
    private float radius = 0.15f;

    [OdinSerialize]
    [MinValue(0.0f)]
    [LabelText("Durée d'apparition")]
    private float spawnDuration = 0.3f;

    [OdinSerialize]
    [MinValue(0.0f)]
    [LabelText("Durée de remplissage")]
    private float fillDuration = 1.0f;

    [NonSerialized] private float nextAttackTime;
    [NonSerialized] private List<CircleDamageZone> activeZones;

    public DummyCircleAttackBehaviour()
    {
    }

    public DummyCircleAttackBehaviour(CircleDamageZone circleDamageZonePrefab, float attackInterval, float radius, float spawnDuration, float fillDuration)
    {
        this.circleDamageZonePrefab = circleDamageZonePrefab;
        this.attackInterval = attackInterval;
        this.radius = radius;
        this.spawnDuration = spawnDuration;
        this.fillDuration = fillDuration;
    }

    public void StartBehaviour(EnemyController enemy, BehaviourExecution execution)
    {
        CancelActiveZones();
        activeZones = new List<CircleDamageZone>();
        nextAttackTime = Time.time + attackInterval;
    }

    public void UpdateBehaviour(EnemyController enemy)
    {
        if (enemy == null || Time.time < nextAttackTime)
            return;

        SpawnCircleAttack(enemy);
        nextAttackTime = Time.time + attackInterval;
    }

    public void FixedUpdateBehaviour(EnemyController enemy)
    {
    }

    public void StopBehaviour(EnemyController enemy)
    {
    }

    public void CancelBehaviour(EnemyController enemy)
    {
        CancelActiveZones();
    }

    public void SetSubBehaviourState(bool state)
    {
    }

    private void SpawnCircleAttack(EnemyController enemy)
    {
        if (circleDamageZonePrefab == null)
        {
            Debug.LogError("[DummyCircleAttackBehaviour] A CircleDamageZone prefab is required.", enemy);
            return;
        }

        CircleDamageZone zone = UnityEngine.Object.Instantiate(
            circleDamageZonePrefab,
            enemy.transform.position,
            Quaternion.Euler(90.0f, 0.0f, 0.0f)
        );

        zone.Setup(radius, spawnDuration, fillDuration);
        activeZones.Add(zone);
    }

    private void CancelActiveZones()
    {
        if (activeZones == null)
            return;

        foreach (CircleDamageZone zone in activeZones)
        {
            if (zone != null)
                zone.Cancel();
        }

        activeZones.Clear();
    }
}
