using System;
using Enemies.Scripts;
using Enemies.Scripts.Behaviours;
using Player.Scripts;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

[Serializable]
public sealed class SekiroCombatObservationBehaviour : IEnemyBehaviour, IContextualEnemyBehaviour
{
    [OdinSerialize, Required, LabelText("Data")]
    private SekiroCombatObservationData _data;

    [NonSerialized] private EnemyController _enemy;
    [NonSerialized] private BehaviourExecution _execution;
    [NonSerialized] private float _completionTime;
    [NonSerialized] private bool _isSubscribed;

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

        _completionTime = Time.time + Mathf.Max(0.0f, _data.Duration);
        PlayAnimation(enemy, _data.ObservationAnimation);
        SubscribeToPlayerAttack();
    }

    public void UpdateBehaviour(EnemyController enemy)
    {
        if (_execution == null || !enemy.IsExecutionActive(_execution))
            return;

        enemy.RefreshContext();
        EnemyContext context = enemy.Context;
        if (context == null || !context.HasTarget)
        {
            _execution.Complete();
            return;
        }

        if (context.DistanceToTarget > _data.PursuitReplaceDistance
            && _data.PursuitData != null
            && enemy.TryReplaceCurrentBehaviour(new SekiroPursuitBehaviour(_data.PursuitData), _execution))
            return;

        FaceAndApproach(enemy, context);

        if (Time.time >= _completionTime)
            _execution.Complete();
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
               && context.DistanceToTarget <= _data.MaximumDistance;
    }

    public float GetWeight(EnemyContext context) => CanExecute(context) ? _data.NormalWeight : 0.0f;

    private void FaceAndApproach(EnemyController enemy, EnemyContext context)
    {
        Vector3 direction = context.DirectionToTarget;
        if (direction.sqrMagnitude > Mathf.Epsilon)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, targetRotation, Time.deltaTime * 8.0f);
        }

        if (!enemy.CanMove || context.DistanceToTarget <= _data.DesiredDistance)
            return;

        float travel = Mathf.Min(
            Mathf.Max(0.0f, context.DistanceToTarget - _data.DesiredDistance),
            Mathf.Max(0.0f, _data.ApproachSpeed) * Time.deltaTime);
        enemy.transform.position += direction * travel;
    }

    private void HandlePlayerAttack(AttackPayload payload)
    {
        if (_enemy == null || _execution == null || _data == null || _data.ParryData == null)
            return;

        _enemy.RefreshContext();
        EnemyContext context = _enemy.Context;
        PlayerStateMachine player = context != null ? context.Player : null;
        GameObject selectedTarget = player != null && player.playerTargeting != null ? player.playerTargeting.Target : null;
        if (player == null
            || !TargetsThisEnemy(selectedTarget)
            || !context.HasTarget
            || context.DistanceToTarget > _data.ParryReactionRange)
            return;

        Vector3 playerFacing = new Vector3(player.LastLookDirection.x, 0.0f, player.LastLookDirection.y).normalized;
        Vector3 playerToEnemy = _enemy.transform.position - player.transform.position;
        playerToEnemy.y = 0.0f;
        if (playerToEnemy.sqrMagnitude <= Mathf.Epsilon
            || Vector3.Dot(playerFacing, playerToEnemy.normalized) < _data.MinimumPlayerFacingDot)
            return;

        _enemy.TryReplaceCurrentBehaviour(new SekiroParryBehaviour(_data.ParryData), _execution);
    }

    private bool TargetsThisEnemy(GameObject selectedTarget)
    {
        return selectedTarget != null
               && _enemy != null
               && (selectedTarget == _enemy.gameObject
                   || selectedTarget.transform.IsChildOf(_enemy.transform)
                   || _enemy.transform.IsChildOf(selectedTarget.transform));
    }

    private void SubscribeToPlayerAttack()
    {
        PlayerStateMachine player = _enemy?.Context?.Player;
        if (_isSubscribed || player?.playerAttack == null)
            return;

        player.playerAttack.OnPlayerAttack.AddListener(HandlePlayerAttack);
        _isSubscribed = true;
    }

    private void ResetRuntimeState()
    {
        UnsubscribeFromPlayerAttack();
        _enemy = null;
        _execution = null;
        _completionTime = 0.0f;
    }

    private void UnsubscribeFromPlayerAttack()
    {
        if (!_isSubscribed)
            return;

        PlayerStateMachine player = _enemy?.Context?.Player;
        if (player?.playerAttack != null)
            player.playerAttack.OnPlayerAttack.RemoveListener(HandlePlayerAttack);

        _isSubscribed = false;
    }

    private static void PlayAnimation(EnemyController enemy, string animationName) => SekiroAnimation.Play(enemy, animationName);
}
