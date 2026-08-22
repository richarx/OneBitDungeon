using System;
using Enemies.Scripts.Behaviours;
using Player.Scripts;
using PrimeTween;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

[Serializable]
public sealed class DummyConeBehaviour : IEnemyBehaviour
{
    [OdinSerialize]
    [Required]
    [LabelText("Data")]
    private ConeTestData data;

    [NonSerialized] private Sequence attackSequence;
    [NonSerialized] private ConeDamageZone currentZone;

    public DummyConeBehaviour()
    {
    }

    public DummyConeBehaviour(ConeTestData data)
    {
        this.data = data;
    }

    public void StartBehaviour(EnemyController enemy, BehaviourExecution execution)
    {
        if (data == null)
        {
            Debug.LogError("[DummyConeBehaviour] Un ConeTestData est requis.", enemy);
            execution.Complete();
            return;
        }

        if (data.ConeDamageZonePrefab == null)
        {
            Debug.LogError("[DummyConeBehaviour] Un prefab de zone conique est requis.", enemy);
            execution.Complete();
            return;
        }

        if (PlayerStateMachine.instance == null)
        {
            Debug.LogError("[DummyConeBehaviour] Aucun joueur n'est disponible.", enemy);
            execution.Complete();
            return;
        }

        SpawnCone(enemy);
        attackSequence = Sequence.Create()
            .ChainDelay(data.AttackInterval)
            .ChainCallback(execution.Complete);
    }

    public void UpdateBehaviour(EnemyController enemy)
    {
        if (currentZone == null || currentZone.IsDestroyed || PlayerStateMachine.instance == null)
            return;

        Vector3 directionToPlayer = PlayerStateMachine.instance.position - currentZone.transform.position;
        directionToPlayer.y = 0.0f;
        currentZone.SetDirection(new Vector2(directionToPlayer.x, directionToPlayer.z));
    }

    public void FixedUpdateBehaviour(EnemyController enemy)
    {
    }

    public void StopBehaviour(EnemyController enemy)
    {
        ResetRuntimeState();
    }

    public void CancelBehaviour(EnemyController enemy)
    {
        ResetRuntimeState();
    }

    public void SetSubBehaviourState(bool state)
    {
    }

    private void SpawnCone(EnemyController enemy)
    {
        Vector3 position = enemy.transform.position;
        Vector3 directionToPlayer = PlayerStateMachine.instance.position - position;
        directionToPlayer.y = 0.0f;

        currentZone = UnityEngine.Object.Instantiate(
            data.ConeDamageZonePrefab,
            position,
            Quaternion.identity);

        currentZone.Setup(
            new Vector2(directionToPlayer.x, directionToPlayer.z),
            data.Radius,
            data.OpeningAngle,
            data.SpawnDuration,
            data.FillDuration);
    }

    private void ResetRuntimeState()
    {
        if (attackSequence.isAlive)
            attackSequence.Stop();

        if (currentZone != null && !currentZone.IsDestroyed)
            currentZone.Cancel();

        attackSequence = default;
        currentZone = null;
    }
}
