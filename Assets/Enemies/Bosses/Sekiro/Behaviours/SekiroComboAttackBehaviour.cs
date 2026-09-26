using System;
using Enemies.Scripts;
using Enemies.Scripts.Behaviours;
using Player.Scripts;
using PrimeTween;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Tools_and_Scripts;
using UnityEngine;

[Serializable]
public sealed class SekiroComboAttackBehaviour : IEnemyBehaviour, IContextualEnemyBehaviour
{
    [OdinSerialize, Required, LabelText("Data")]
    private SekiroComboAttackData _data;

    [NonSerialized] private EnemyController _enemy;
    [NonSerialized] private BehaviourExecution _execution;
    [NonSerialized] private Sequence _sequence;
    private const float _rectangleDamageActivationDelay = 0.05f;

    [NonSerialized] private ConeDamageZone _currentConeZone;
    [NonSerialized] private RectangleDamageZone _currentRectangleZone;
    [NonSerialized] private GameObject _currentRectangleZoneRoot;
    [NonSerialized] private bool _isSubscribed;
    [NonSerialized] private bool _rapidParryWindowOpen;
    [NonSerialized] private float _rapidParryWindowEndTime;

    public void StartBehaviour(EnemyController enemy, BehaviourExecution execution)
    {
        ResetRuntimeState();
        _enemy = enemy;
        _execution = execution;
        enemy.RefreshContext();

        if (_data == null || !CanExecute(enemy.Context) || _data.ConeZonePrefab == null || !HasRapidRectanglePrefabs())
        {
            execution.Complete();
            return;
        }

        SubscribeToPlayerAttack();
        RunRapidAttack(_data.RapidAttack1, 1);
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
               && context.DistanceToTarget <= _data.MaximumStartingDistance;
    }

    public float GetWeight(EnemyContext context) => CanExecute(context) ? _data.NormalWeight : 0.0f;

    private void RunRapidAttack(SekiroAttackStep attack, int index)
    {
        if (TryReplaceWithPursuitIfTooFar())
            return;

        RunAttackStep(attack, true, true, () =>
        {
            if (index == 1)
                RunRapidAttack(_data.RapidAttack2, 2);
            else
                RunVariant();
        });
    }

    private void RunVariant()
    {
        if (TryReplaceWithPursuitIfTooFar())
            return;

        bool useVariantA = PickVariantA();
        if (useVariantA)
        {
            // This animation is deliberately started at the branch point, before the long cone telegraph.
            RunAttackStep(_data.VariantALongCone, false, false, CompleteCurrentExecution);
            return;
        }

        // This animation is deliberately started at the branch point, before the medium cone telegraph.
        RunAttackStep(_data.VariantBMediumCone, false, false, () => RunAttackStep(_data.VariantBFinalRapid, false, true, CompleteCurrentExecution));
    }

    private void RunAttackStep(SekiroAttackStep attack, bool opensParryWindow, bool usesRectangleZone, Action onFinished)
    {
        if (attack == null || _enemy == null || _execution == null)
        {
            CompleteCurrentExecution();
            return;
        }

        CancelCurrentZone();
        _rapidParryWindowOpen = opensParryWindow;
        _rapidParryWindowEndTime = opensParryWindow
            ? Time.time + Mathf.Max(0.0f, _data.RapidParryWindowDuration)
            : 0.0f;
        PlayAnimation(_enemy, attack.PreparationAnimation);
        SpawnAttackZone(attack, usesRectangleZone);

        _sequence = Sequence.Create()
            .ChainDelay(attack.SpawnDuration + attack.FillDuration + (usesRectangleZone ? _rectangleDamageActivationDelay : 0.0f))
            .ChainCallback(() =>
            {
                _rapidParryWindowOpen = false;
                PlayAnimation(_enemy, attack.ImpactAnimation);
            })
            .ChainDelay(attack.RecoveryDuration)
            .ChainCallback(onFinished);
    }

    private void SpawnAttackZone(SekiroAttackStep attack, bool usesRectangleZone)
    {
        _enemy.RefreshContext();
        EnemyContext context = _enemy.Context;
        if (context == null || !context.HasTarget)
            return;

        if (usesRectangleZone)
        {
            SpawnRectangle(attack, context);
            return;
        }

        if (_data.ConeZonePrefab == null)
            return;

        _currentConeZone = UnityEngine.Object.Instantiate(_data.ConeZonePrefab, _enemy.transform.position, Quaternion.identity);
        _currentConeZone.Setup(
            new Vector2(context.DirectionToTarget.x, context.DirectionToTarget.z),
            attack.Radius,
            attack.OpeningAngle,
            attack.SpawnDuration,
            attack.FillDuration,
            null,
            attack.StaggerPower);

        DealDamageToPlayer damageDealer = _currentConeZone.GetComponent<DealDamageToPlayer>();
        if (damageDealer != null)
            damageDealer.Configure(attack.Damage, attack.CanBeParried, attack.CanBeJumped);
    }

    private void SpawnRectangle(SekiroAttackStep attack, EnemyContext context)
    {
        if (attack.RectangleZonePrefab == null)
            return;

        _currentRectangleZoneRoot = UnityEngine.Object.Instantiate(attack.RectangleZonePrefab, _enemy.transform.position, Quaternion.identity);
        _currentRectangleZone = _currentRectangleZoneRoot.GetComponentInChildren<RectangleDamageZone>();
        if (_currentRectangleZone == null)
        {
            UnityEngine.Object.Destroy(_currentRectangleZoneRoot);
            _currentRectangleZoneRoot = null;
            return;
        }

        Vector2 direction = new Vector2(context.DirectionToTarget.x, context.DirectionToTarget.z).normalized;
        _currentRectangleZoneRoot.transform.rotation = Quaternion.LookRotation(direction.AddAngleToDirection(90.0f).ToVector3());
        _currentRectangleZone.SetDimensions(attack.RectangleWidth, attack.RectangleLength);
        _currentRectangleZone.Setup(Vector2.right, attack.SpawnDuration, attack.FillDuration, null, attack.StaggerPower);

        DealDamageToPlayer damageDealer = _currentRectangleZone.GetComponent<DealDamageToPlayer>();
        if (damageDealer != null)
            damageDealer.Configure(attack.Damage, attack.CanBeParried, attack.CanBeJumped);
    }

    private void HandlePlayerAttack(AttackPayload payload)
    {
        if (!_rapidParryWindowOpen
            || Time.time > _rapidParryWindowEndTime
            || _enemy == null
            || _execution == null
            || _data == null
            || _data.ParryData == null)
            return;

        _enemy.RefreshContext();
        if (!CanReactToThisPlayerAttack(_enemy.Context))
            return;

        _enemy.TryReplaceCurrentBehaviour(new SekiroParryBehaviour(_data.ParryData), _execution);
    }

    private bool CanReactToThisPlayerAttack(EnemyContext context)
    {
        PlayerStateMachine player = context != null ? context.Player : null;
        GameObject selectedTarget = player != null && player.playerTargeting != null ? player.playerTargeting.Target : null;
        if (player == null
            || !TargetsThisEnemy(selectedTarget)
            || !context.HasTarget
            || context.DistanceToTarget > _data.ParryReactionRange)
            return false;

        Vector3 playerFacing = new Vector3(player.LastLookDirection.x, 0.0f, player.LastLookDirection.y).normalized;
        Vector3 playerToEnemy = (_enemy.transform.position - player.transform.position);
        playerToEnemy.y = 0.0f;
        return playerToEnemy.sqrMagnitude > Mathf.Epsilon
               && Vector3.Dot(playerFacing, playerToEnemy.normalized) >= _data.MinimumPlayerFacingDot;
    }

    private bool TargetsThisEnemy(GameObject selectedTarget)
    {
        return selectedTarget != null
               && _enemy != null
               && (selectedTarget == _enemy.gameObject
                   || selectedTarget.transform.IsChildOf(_enemy.transform)
                   || _enemy.transform.IsChildOf(selectedTarget.transform));
    }

    private bool TryReplaceWithPursuitIfTooFar()
    {
        if (_enemy == null || _execution == null || _data == null || _data.PursuitData == null)
            return false;

        _enemy.RefreshContext();
        EnemyContext context = _enemy.Context;
        return context != null
               && context.HasTarget
               && context.DistanceToTarget > _data.PursuitReplaceDistance
               && _enemy.TryReplaceCurrentBehaviour(new SekiroPursuitBehaviour(_data.PursuitData), _execution);
    }

    private bool PickVariantA()
    {
        float a = Mathf.Max(0.0f, _data.VariantAWeight);
        float b = Mathf.Max(0.0f, _data.VariantBWeight);
        return a + b <= Mathf.Epsilon || UnityEngine.Random.value * (a + b) < a;
    }

    private void CompleteCurrentExecution()
    {
        if (_enemy != null && _execution != null && _enemy.IsExecutionActive(_execution))
            _execution.Complete();
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

        if (_sequence.isAlive)
            _sequence.Stop();

        CancelCurrentZone();
        _sequence = default;
        _enemy = null;
        _execution = null;
        _rapidParryWindowOpen = false;
        _rapidParryWindowEndTime = 0.0f;
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

    private void CancelCurrentZone()
    {
        if (_currentConeZone != null && !_currentConeZone.IsDestroyed)
            _currentConeZone.Cancel();

        if (_currentRectangleZone != null)
            _currentRectangleZone.Cancel();

        if (_currentRectangleZoneRoot != null)
            UnityEngine.Object.Destroy(_currentRectangleZoneRoot);

        _currentConeZone = null;
        _currentRectangleZone = null;
        _currentRectangleZoneRoot = null;
    }

    private bool HasRapidRectanglePrefabs()
    {
        return _data.RapidAttack1?.RectangleZonePrefab != null
               && _data.RapidAttack2?.RectangleZonePrefab != null
               && _data.VariantBFinalRapid?.RectangleZonePrefab != null;
    }

    private static void PlayAnimation(EnemyController enemy, string animationName) => SekiroAnimation.Play(enemy, animationName);
}
