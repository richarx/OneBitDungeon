using System;
using System.Collections.Generic;
using Enemies.Scripts.Behaviours;
using PrimeTween;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

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

    [NonSerialized] private Sequence attackSequence;

    [NonSerialized] private CircleDamageZone circleDamageZone;

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

        attackSequence = Sequence.Create()
        .ChainCallback(() => SpawnCircleAttack(enemy))
        .ChainDelay(attackInterval)
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
        if (attackSequence.isAlive)
            attackSequence.Stop();
        if (circleDamageZone != null && !circleDamageZone.IsDestroyed)
            circleDamageZone.Cancel();
    }

    public void CancelBehaviour(EnemyController enemy)
    {
        if (attackSequence.isAlive)
            attackSequence.Stop();

        if (circleDamageZone != null)
            circleDamageZone.Cancel();
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
        circleDamageZone = zone;
    }

}
