using System;
using Enemies.Scripts;
using Enemies.Scripts.Behaviours;
using PrimeTween;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

[Serializable]
public sealed class SekiroPursuitBehaviour : IEnemyBehaviour, IContextualEnemyBehaviour
{
    [OdinSerialize, Required, LabelText("Data")]
    private SekiroPursuitData _data;

    [NonSerialized] private Sequence _sequence;
    [NonSerialized] private ConeDamageZone _dashZone;
    [NonSerialized] private EnemyController _enemy;
    [NonSerialized] private BehaviourExecution _execution;

    public SekiroPursuitBehaviour()
    {
    }

    public SekiroPursuitBehaviour(SekiroPursuitData data)
    {
        _data = data;
    }

    public void StartBehaviour(EnemyController enemy, BehaviourExecution execution)
    {
        ResetRuntimeState();
        _enemy = enemy;
        _execution = execution;
        enemy.RefreshContext();

        if (_data == null || !CanExecute(enemy.Context))
        {
            execution.Complete();
            return;
        }

        EnemyContext context = enemy.Context;
        float travelDistance = Mathf.Min(
            Mathf.Max(0.0f, context.DistanceToTarget - _data.DesiredDistance),
            Mathf.Max(0.0f, _data.MaximumDashDistance));
        Vector3 destination = enemy.transform.position + context.DirectionToTarget * travelDistance;
        bool hasDashAttack = _data.DashAttackEnabled && _data.DashConePrefab != null && _data.DashAttack != null;
        PlayAnimation(enemy, _data.DashAnimation);
        enemy.afterImage?.Trigger(_data.DashDuration);

        _sequence = Sequence.Create()
            .Chain(EnemyMovementUtility.CreateMoveToPosition(enemy, destination, _data.DashDuration, Ease.OutCirc))
            .ChainDelay(_data.AttackDelay)
            .ChainCallback(() => SpawnDashAttack(enemy))
            .ChainDelay(hasDashAttack ? _data.DashAttack.SpawnDuration + _data.DashAttack.FillDuration : 0.0f)
            .ChainCallback(() =>
            {
                if (hasDashAttack)
                    PlayAnimation(enemy, _data.DashAttack.ImpactAnimation);
            })
            .ChainDelay(hasDashAttack ? _data.DashAttack.RecoveryDuration : 0.0f)
            .ChainCallback(execution.Complete);
    }

    public void UpdateBehaviour(EnemyController enemy)
    {
    }

    public void FixedUpdateBehaviour(EnemyController enemy)
    {
    }

    public void StopBehaviour(EnemyController enemy) => ResetRuntimeState();

    public void CancelBehaviour(EnemyController enemy) => ResetRuntimeState();

    public void SetSubBehaviourState(bool state)
    {
    }

    public bool CanExecute(EnemyContext context)
    {
        return _data != null
               && context != null
               && context.HasTarget
               && context.DistanceToTarget >= _data.ActivationDistance;
    }

    public float GetWeight(EnemyContext context)
    {
        if (!CanExecute(context))
            return 0.0f;

        float weight = _data.BaseWeight + Mathf.Max(0.0f, context.DistanceToTarget - _data.DesiredDistance) * _data.WeightPerMeter;
        if (Time.unscaledTime - context.LastRollStartUnscaledTime <= _data.DistantRollMemoryDuration
            && context.DistanceAtRollStart >= _data.ActivationDistance)
            weight += _data.DistantRollWeightBonus;

        return weight;
    }

    private void SpawnDashAttack(EnemyController enemy)
    {
        if (_data == null || !_data.DashAttackEnabled || _data.DashConePrefab == null || _data.DashAttack == null)
            return;

        enemy.RefreshContext();
        EnemyContext context = enemy.Context;
        if (context == null || !context.HasTarget)
            return;

        SekiroAttackStep attack = _data.DashAttack;
        _dashZone = UnityEngine.Object.Instantiate(_data.DashConePrefab, enemy.transform.position, Quaternion.identity);
        _dashZone.Setup(
            new Vector2(context.DirectionToTarget.x, context.DirectionToTarget.z),
            attack.Radius,
            attack.OpeningAngle,
            attack.SpawnDuration,
            attack.FillDuration,
            null,
            attack.StaggerPower);

        DealDamageToPlayer damageDealer = _dashZone.GetComponent<DealDamageToPlayer>();
        if (damageDealer != null)
            damageDealer.Configure(attack.Damage, attack.CanBeParried, attack.CanBeJumped);

        PlayAnimation(enemy, attack.PreparationAnimation);
    }

    private void ResetRuntimeState()
    {
        if (_sequence.isAlive)
            _sequence.Stop();

        if (_dashZone != null && !_dashZone.IsDestroyed)
            _dashZone.Cancel();

        _sequence = default;
        _dashZone = null;
        _enemy = null;
        _execution = null;
    }

    private static void PlayAnimation(EnemyController enemy, string animationName) => SekiroAnimation.Play(enemy, animationName);
}
