using System;
using Enemies.Scripts;
using Enemies.Scripts.Behaviours;
using Player.Scripts;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

[Serializable]
public sealed class SekiroParryBehaviour : IEnemyBehaviour, IContextualEnemyBehaviour
{
    [OdinSerialize, Required, LabelText("Data")]
    private SekiroParryData _data;

    [NonSerialized] private EnemyController _enemy;
    [NonSerialized] private BehaviourExecution _execution;
    [NonSerialized] private ConeDamageZone _revengeZone;
    [NonSerialized] private PlayerAttack _playerAttack;
    [NonSerialized] private bool _isDamageInterceptionSubscribed;
    [NonSerialized] private bool _isPlayerAttackSubscribed;
    [NonSerialized] private bool _parrySucceeded;
    [NonSerialized] private bool _revengeSpawned;
    [NonSerialized] private float _parryWindowEndTime;
    [NonSerialized] private float _revengeAtTime;
    [NonSerialized] private float _revengeImpactAtTime;
    [NonSerialized] private float _completionTime;
    [NonSerialized] private bool _revengeImpactPlayed;

    public SekiroParryBehaviour()
    {
    }

    public SekiroParryBehaviour(SekiroParryData data)
    {
        _data = data;
    }

    public void StartBehaviour(EnemyController enemy, BehaviourExecution execution)
    {
        ResetRuntimeState();
        _enemy = enemy;
        _execution = execution;

        if (_data == null || enemy == null || enemy.damageable == null)
        {
            execution.Complete();
            return;
        }

        enemy.RefreshContext();
        _playerAttack = enemy.Context?.Player?.playerAttack;
        SubscribeToPlayerAttack();
        RearmParryCycle();
    }

    public void UpdateBehaviour(EnemyController enemy)
    {
        if (_execution == null || !enemy.IsExecutionActive(_execution))
            return;

        if (!_parrySucceeded)
        {
            if (Time.time >= _parryWindowEndTime)
            {
                PlayAnimation(enemy, _data.RecoveryAnimation);
                _execution.Complete();
            }

            return;
        }

        if (!_revengeSpawned && _data.RevengeEnabled && Time.time >= _revengeAtTime)
            SpawnRevenge(enemy);

        if (_revengeSpawned && !_revengeImpactPlayed && Time.time >= _revengeImpactAtTime)
        {
            _revengeImpactPlayed = true;
            PlayAnimation(enemy, _data.RevengeAttack?.ImpactAnimation);
        }

        if (Time.time >= _completionTime)
        {
            PlayAnimation(enemy, _data.RecoveryAnimation);
            _execution.Complete();
        }
    }

    public void FixedUpdateBehaviour(EnemyController enemy)
    {
    }

    public void StopBehaviour(EnemyController enemy) => ResetRuntimeState();

    public void CancelBehaviour(EnemyController enemy) => ResetRuntimeState();

    public void SetSubBehaviourState(bool state)
    {
    }

    public bool CanExecute(EnemyContext context) => _data != null && context != null && context.HasTarget;

    public float GetWeight(EnemyContext context) => _data != null ? _data.NormalWeight : 0.0f;

    private bool TryInterceptDamage(AttackPayload payload, Vector2 direction)
    {
        if (_data == null || !_isDamageInterceptionSubscribed || _parrySucceeded || Time.time > _parryWindowEndTime)
            return false;

        if (payload != null && payload.Type == AttackType.Critical && !_data.CanParryCriticalAttacks)
            return false;

        _parrySucceeded = true;
        UnsubscribeDamageInterception();
        _enemy?.RecordExchangeResult(EnemyExchangeResult.PlayerDeflected);
        PlayAnimation(_enemy, _data.SuccessAnimation);

        float revengeDuration = _data.RevengeEnabled && _data.RevengeConePrefab != null && _data.RevengeAttack != null
            ? _data.RevengeAttack.TotalDuration
            : 0.0f;
        _revengeAtTime = Time.time + Mathf.Max(0.0f, _data.RevengeDelay);
        _revengeImpactAtTime = _revengeAtTime + (_data.RevengeAttack != null
            ? _data.RevengeAttack.SpawnDuration + _data.RevengeAttack.FillDuration
            : 0.0f);
        _completionTime = _revengeAtTime + revengeDuration + Mathf.Max(0.0f, _data.FinalRecoveryDuration);
        return true;
    }

    private void HandlePlayerAttack(AttackPayload payload)
    {
        if (_data == null
            || _enemy == null
            || _execution == null
            || _revengeSpawned
            || !_enemy.IsExecutionActive(_execution)
            || !CanParry(payload))
            return;

        _enemy.RefreshContext();
        if (!CanReactToThisPlayerAttack(_enemy.Context))
            return;

        RearmParryCycle();
    }

    private void RearmParryCycle()
    {
        if (_enemy == null || _data == null)
            return;

        _parrySucceeded = false;
        _revengeAtTime = 0.0f;
        _revengeImpactAtTime = 0.0f;
        _completionTime = 0.0f;
        _revengeImpactPlayed = false;
        _parryWindowEndTime = Time.time + Mathf.Max(0.0f, _data.ParryWindowDuration);
        EnsureDamageInterceptionSubscription();
        PlayAnimation(_enemy, _data.StartAnimation);
    }

    private bool CanReactToThisPlayerAttack(EnemyContext context)
    {
        PlayerStateMachine player = context != null ? context.Player : null;
        GameObject selectedTarget = player != null && player.playerTargeting != null ? player.playerTargeting.Target : null;
        if (player == null
            || !TargetsThisEnemy(selectedTarget)
            || !context.HasTarget
            || context.DistanceToTarget > _data.RearmReactionRange)
            return false;

        Vector3 playerFacing = new Vector3(player.LastLookDirection.x, 0.0f, player.LastLookDirection.y).normalized;
        Vector3 playerToEnemy = _enemy.transform.position - player.transform.position;
        playerToEnemy.y = 0.0f;
        return playerToEnemy.sqrMagnitude > Mathf.Epsilon
               && Vector3.Dot(playerFacing, playerToEnemy.normalized) >= _data.RearmMinimumPlayerFacingDot;
    }

    private bool TargetsThisEnemy(GameObject selectedTarget)
    {
        return selectedTarget != null
               && _enemy != null
               && (selectedTarget == _enemy.gameObject
                   || selectedTarget.transform.IsChildOf(_enemy.transform)
                   || _enemy.transform.IsChildOf(selectedTarget.transform));
    }

    private bool CanParry(AttackPayload payload)
    {
        return payload == null
               || payload.Type != AttackType.Critical
               || _data.CanParryCriticalAttacks;
    }

    private void SpawnRevenge(EnemyController enemy)
    {
        if (!_data.RevengeEnabled || _data.RevengeConePrefab == null || _data.RevengeAttack == null)
            return;

        enemy.RefreshContext();
        EnemyContext context = enemy.Context;
        if (context == null || !context.HasTarget)
            return;

        _revengeSpawned = true;

        Vector3 position = enemy.transform.position;
        _revengeZone = UnityEngine.Object.Instantiate(_data.RevengeConePrefab, position, Quaternion.identity);
        _revengeZone.Setup(
            new Vector2(context.DirectionToTarget.x, context.DirectionToTarget.z),
            _data.RevengeAttack.Radius,
            _data.RevengeAttack.OpeningAngle,
            _data.RevengeAttack.SpawnDuration,
            _data.RevengeAttack.FillDuration,
            null,
            _data.RevengeAttack.StaggerPower);

        DealDamageToPlayer damageDealer = _revengeZone.GetComponent<DealDamageToPlayer>();
        if (damageDealer != null)
            damageDealer.Configure(_data.RevengeAttack.Damage, _data.RevengeAttack.CanBeParried, _data.RevengeAttack.CanBeJumped);

        // The zone includes the close-dodge window and colour transition before its real impact.
        _revengeImpactAtTime = Time.time + _revengeZone.DamageDelay;
        _completionTime = _revengeImpactAtTime + Mathf.Max(0.0f, _data.RevengeAttack.RecoveryDuration)
            + Mathf.Max(0.0f, _data.FinalRecoveryDuration);

        PlayAnimation(enemy, _data.RevengeAttack.PreparationAnimation);
    }

    private void ResetRuntimeState()
    {
        UnsubscribeDamageInterception();
        UnsubscribeFromPlayerAttack();

        if (_revengeZone != null && !_revengeZone.IsDestroyed)
            _revengeZone.Cancel();

        _enemy = null;
        _execution = null;
        _playerAttack = null;
        _revengeZone = null;
        _parrySucceeded = false;
        _revengeSpawned = false;
        _parryWindowEndTime = 0.0f;
        _revengeAtTime = 0.0f;
        _revengeImpactAtTime = 0.0f;
        _completionTime = 0.0f;
        _revengeImpactPlayed = false;
    }

    private void UnsubscribeDamageInterception()
    {
        if (!_isDamageInterceptionSubscribed)
            return;

        if (_enemy != null && _enemy.damageable != null)
            _enemy.damageable.OnTryInterceptDamage -= TryInterceptDamage;

        _isDamageInterceptionSubscribed = false;
    }

    private void EnsureDamageInterceptionSubscription()
    {
        if (_isDamageInterceptionSubscribed || _enemy?.damageable == null)
            return;

        _enemy.damageable.OnTryInterceptDamage += TryInterceptDamage;
        _isDamageInterceptionSubscribed = true;
    }

    private void SubscribeToPlayerAttack()
    {
        if (_isPlayerAttackSubscribed || _playerAttack == null)
            return;

        _playerAttack.OnPlayerAttack.AddListener(HandlePlayerAttack);
        _isPlayerAttackSubscribed = true;
    }

    private void UnsubscribeFromPlayerAttack()
    {
        if (!_isPlayerAttackSubscribed)
            return;

        _playerAttack?.OnPlayerAttack.RemoveListener(HandlePlayerAttack);
        _isPlayerAttackSubscribed = false;
    }

    private static void PlayAnimation(EnemyController enemy, string animationName) => SekiroAnimation.Play(enemy, animationName);
}
