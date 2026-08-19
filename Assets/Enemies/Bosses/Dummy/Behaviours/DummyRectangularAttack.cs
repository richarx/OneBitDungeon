using System;
using System.Collections.Generic;
using Enemies.Scripts.Behaviours;
using Player.Scripts;
using PrimeTween;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Tools_and_Scripts;
using UnityEngine;

[Serializable]
public sealed class DummyRectangularAttackBehaviour : IEnemyBehaviour
{
    [OdinSerialize]
    [Required]
    [LabelText("Prefab de zone rect")]
    private GameObject rectangularDamageZonePrefab;

    [OdinSerialize]
    [MinValue(0.0f)]
    [LabelText("Intervalle d'attaque")]
    private float attackInterval = 4.0f;

    [OdinSerialize]
    [MinValue(0.0f)]
    [LabelText("Durée d'apparition")]
    private float spawnDuration = 0.3f;

    [OdinSerialize]
    [MinValue(0.0f)]
    [LabelText("Durée de remplissage")]
    private float fillDuration = 1.0f;

    [OdinSerialize]
    [MinValue(0.0f)]
    [LabelText("Rotation Dampening")]
    private float dampening = 0.2f;

    [NonSerialized] private Sequence attackSequence;

    [NonSerialized] private RectangleDamageZone currentZone;

    public DummyRectangularAttackBehaviour()
    {
    }

    public DummyRectangularAttackBehaviour(GameObject rectangularDamageZonePrefab, float attackInterval, float spawnDuration, float fillDuration)
    {
        this.rectangularDamageZonePrefab = rectangularDamageZonePrefab;
        this.attackInterval = attackInterval;
        this.spawnDuration = spawnDuration;
        this.fillDuration = fillDuration;
    }

    public void StartBehaviour(EnemyController enemy, BehaviourExecution execution)
    {

        attackSequence = Sequence.Create()
            .ChainCallback(() => SpawnRectangularAttack(enemy))
            .ChainDelay(attackInterval)
            .ChainCallback(() => execution.Complete());

    }

    public void UpdateBehaviour(EnemyController enemy)
    {
        if (currentZone != null)
            RotateThrowTowardPlayer();
    }

    private Vector3 RotateThrowTowardPlayer()
    {
        Vector3 position = currentZone.transform.parent.position;
        Vector3 direction = (PlayerStateMachine.instance.position - position).normalized;

        currentZone.transform.parent.rotation = Quaternion.Slerp(currentZone.transform.parent.rotation, Quaternion.LookRotation(direction.ToVector2().AddAngleToDirection(90.0f).ToVector3()), Time.deltaTime / dampening);

        return direction;
    }

    public void FixedUpdateBehaviour(EnemyController enemy)
    {
    }

    public void StopBehaviour(EnemyController enemy)
    {
    }

    public void CancelBehaviour(EnemyController enemy)
    {
        if (attackSequence.isAlive)
            attackSequence.Stop();


    }

    public void SetSubBehaviourState(bool state)
    {
    }

    private void SpawnRectangularAttack(EnemyController enemy)
    {
        if (rectangularDamageZonePrefab == null)
        {
            Debug.LogError("[DummyRectangularAttackBehaviour] A RectangleDamageZone prefab is required.", enemy);
            return;
        }

        GameObject zoneObject = UnityEngine.Object.Instantiate(
            rectangularDamageZonePrefab,
            enemy.transform.position,
            Quaternion.Euler(90.0f, 0.0f, 0.0f)
        );
        currentZone = zoneObject.GetComponentInChildren<RectangleDamageZone>();

        currentZone.Setup(Vector2.right, spawnDuration, fillDuration);
    }

}
