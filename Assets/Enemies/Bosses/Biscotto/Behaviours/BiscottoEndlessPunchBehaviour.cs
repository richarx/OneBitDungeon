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
public sealed class BiscottoEndlessPunchBehaviour : IEnemyBehaviour
{
    private const float _minimumDirectionSqrMagnitude = 0.0001f;

    private sealed class ActivePunch
    {
        public RectangleDamageZone DamageZone { get; }
        public Transform Root { get; }
        public float AimEndTimestamp { get; }
        public BiscottoPunchSide Side { get; }

        public ActivePunch(
            RectangleDamageZone damageZone,
            Transform root,
            float aimEndTimestamp,
            BiscottoPunchSide side)
        {
            DamageZone = damageZone;
            Root = root;
            AimEndTimestamp = aimEndTimestamp;
            Side = side;
        }
    }

    [OdinSerialize]
    [Required]
    [LabelText("Data")]
    private BiscottoEndlessPunchData _data;

    [NonSerialized] private Sequence _setupSequence;
    [NonSerialized] private Sequence _recoverySequence;
    [NonSerialized] private List<ActivePunch> _activePunches = new List<ActivePunch>();
    [NonSerialized] private EnemyController _currentEnemy;
    [NonSerialized] private BehaviourExecution _currentExecution;
    [NonSerialized] private PlayerStateMachine _player;
    [NonSerialized] private float _nextPunchTimestamp;
    [NonSerialized] private int _launchedPunchCount;
    [NonSerialized] private bool _isBarrageActive;
    [NonSerialized] private bool _isStopping;

    public void StartBehaviour(EnemyController enemy, BehaviourExecution execution)
    {
        ResetRuntimeState();

        if (_data == null)
        {
            Debug.LogError("[BiscottoEndlessPunchBehaviour] Un data de rafale infinie est requis.", enemy);
            execution.Complete();
            return;
        }

        if (_data.RectangularDamageZonePrefab == null)
        {
            Debug.LogError("[BiscottoEndlessPunchBehaviour] Un prefab de zone rectangulaire est requis.", enemy);
            execution.Complete();
            return;
        }

        if (enemy.damageable == null)
        {
            Debug.LogError("[BiscottoEndlessPunchBehaviour] Damageable est requis sur Biscotto.", enemy);
            execution.Complete();
            return;
        }

        _player = PlayerStateMachine.instance;
        if (_player == null)
        {
            Debug.LogError("[BiscottoEndlessPunchBehaviour] Aucun joueur n'est disponible.", enemy);
            execution.Complete();
            return;
        }

        _currentEnemy = enemy;
        _currentExecution = execution;
        enemy.damageable.OnTakeDamage.AddListener(HandleEnemyHit);

        PushPlayerToFront();
        BeginMoveToBack(enemy);

        Vector3 backPosition = ToWorldPosition(_data.BackPosition, enemy.transform.position.y);
        _setupSequence = Sequence.Create()
            .Chain(BiscottoMovementUtility.CreateLinearMove(
                enemy,
                backPosition,
                _data.MoveToBackDuration,
                Ease.InOutSine))
            .ChainCallback(BeginBarrage);
    }

    public void UpdateBehaviour(EnemyController enemy)
    {
        UpdateActivePunches();

        if (!_isBarrageActive || _isStopping || Time.time < _nextPunchTimestamp)
            return;

        LaunchPunch(enemy);
        _nextPunchTimestamp = Time.time + _data.PunchInterval;
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

    private void PushPlayerToFront()
    {
        Vector3 frontPosition = ToWorldPosition(_data.FrontPosition, _player.position.y);
        Vector3 backPosition = ToWorldPosition(_data.BackPosition, _player.position.y);
        Vector3 pushDirection = frontPosition - backPosition;
        pushDirection.y = 0.0f;

        if (pushDirection.sqrMagnitude <= _minimumDirectionSqrMagnitude)
            pushDirection = Vector3.back;

        _player.playerStagger.TriggerStagger(_player, pushDirection, _data.PlayerPushPower);
    }

    private void BeginMoveToBack(EnemyController enemy)
    {
        PlayAnimation(enemy, _data.MoveAnimation);

        if (_data.TriggerAfterImageOnMove && enemy.afterImage != null)
            enemy.afterImage.Trigger(_data.MoveToBackDuration);
    }

    private void BeginBarrage()
    {
        if (_isStopping
            || _currentEnemy == null
            || !_currentEnemy.IsExecutionActive(_currentExecution))
        {
            return;
        }

        _isBarrageActive = true;
        _nextPunchTimestamp = Time.time;
    }

    private void LaunchPunch(EnemyController enemy)
    {
        PlayAnimation(enemy, _data.PunchAnimation);
        BiscottoPunchSide punchSide = GetPunchSide();

        GameObject zoneObject = UnityEngine.Object.Instantiate(
            _data.RectangularDamageZonePrefab,
            enemy.transform.position + ComputeDamageZoneOffset(enemy, punchSide),
            Quaternion.identity);
        RectangleDamageZone damageZone = zoneObject.GetComponentInChildren<RectangleDamageZone>();

        if (damageZone == null)
        {
            Debug.LogError(
                $"[{enemy.name}] Le prefab '{_data.RectangularDamageZonePrefab.name}' ne contient pas de RectangleDamageZone.",
                _data.RectangularDamageZonePrefab);
            UnityEngine.Object.Destroy(zoneObject);
            return;
        }

        ActivePunch punch = new ActivePunch(
            damageZone,
            zoneObject.transform,
            Time.time + Mathf.Max(
                0.0f,
                _data.SpawnDuration + _data.FillDuration - _data.LockBeforeImpact),
            punchSide);

        _activePunches.Add(punch);
        damageZone.SetDimensions(_data.DamageZoneWidth, _data.DamageZoneLength);
        RotatePunchTowardPlayer(punch, true);
        damageZone.Setup(Vector2.right, _data.SpawnDuration, _data.FillDuration);
        _launchedPunchCount++;
    }

    private Vector3 ComputeDamageZoneOffset(EnemyController enemy, BiscottoPunchSide punchSide)
    {
        if (_data.DamageZoneSideOffset <= 0.0f)
            return Vector3.zero;

        Vector3 directionToPlayer = _player.position - enemy.transform.position;
        directionToPlayer.y = 0.0f;

        if (directionToPlayer.sqrMagnitude <= _minimumDirectionSqrMagnitude)
            directionToPlayer = enemy.transform.forward;

        directionToPlayer.Normalize();
        Vector3 rightSide = new Vector3(directionToPlayer.z, 0.0f, -directionToPlayer.x);
        return rightSide * GetSideSign(punchSide) * _data.DamageZoneSideOffset;
    }

    private BiscottoPunchSide GetPunchSide()
    {
        bool isEvenPunch = _launchedPunchCount % 2 == 0;
        if (isEvenPunch)
            return _data.FirstPunchSide;

        return _data.FirstPunchSide == BiscottoPunchSide.Left
            ? BiscottoPunchSide.Right
            : BiscottoPunchSide.Left;
    }

    private static float GetSideSign(BiscottoPunchSide side)
    {
        return side == BiscottoPunchSide.Right ? 1.0f : -1.0f;
    }

    private void UpdateActivePunches()
    {
        for (int index = _activePunches.Count - 1; index >= 0; index--)
        {
            ActivePunch punch = _activePunches[index];
            if (punch == null || punch.DamageZone == null || punch.Root == null)
            {
                _activePunches.RemoveAt(index);
                continue;
            }

            if (Time.time < punch.AimEndTimestamp)
                RotatePunchTowardPlayer(punch);
        }
    }

    private void RotatePunchTowardPlayer(ActivePunch punch, bool immediate = false)
    {
        if (punch == null || punch.Root == null || _player == null)
            return;

        Vector3 directionToPlayer = _player.position - punch.Root.position;
        directionToPlayer.y = 0.0f;
        if (directionToPlayer.sqrMagnitude <= _minimumDirectionSqrMagnitude)
            return;

        Vector3 direction = directionToPlayer;
        float corridorOffset = Mathf.Max(0.0f, _data.DodgeCorridorOffset);
        if (corridorOffset > 0.0f)
        {
            Vector3 rightSide = new Vector3(directionToPlayer.z, 0.0f, -directionToPlayer.x).normalized;
            Vector3 aimPosition = _player.position
                                  + rightSide * GetSideSign(punch.Side) * corridorOffset;
            direction = aimPosition - punch.Root.position;
            direction.y = 0.0f;
        }

        Quaternion targetRotation = Quaternion.LookRotation(
            direction.normalized.ToVector2().AddAngleToDirection(90.0f).ToVector3());

        if (immediate)
        {
            punch.Root.rotation = targetRotation;
            return;
        }

        punch.Root.rotation = Quaternion.Slerp(
            punch.Root.rotation,
            targetRotation,
            Time.deltaTime / Mathf.Max(0.001f, _data.RotationDampening));
    }

    private void HandleEnemyHit(Vector2 direction)
    {
        if (_isStopping
            || _currentEnemy == null
            || !_currentEnemy.IsExecutionActive(_currentExecution))
        {
            return;
        }

        _isStopping = true;
        _isBarrageActive = false;

        if (_setupSequence.isAlive)
            _setupSequence.Stop();

        CancelActivePunches();
        PlayAnimation(_currentEnemy, _data.HitAnimation);

        _recoverySequence = Sequence.Create()
            .ChainDelay(_data.HitRecoveryDuration)
            .ChainCallback(() => PlayAnimation(_currentEnemy, "Idle"))
            .ChainCallback(() =>
            {
                if (_currentEnemy != null && _currentEnemy.IsExecutionActive(_currentExecution))
                    _currentExecution.Complete();
            });
    }

    private void CancelActivePunches()
    {
        if (_activePunches == null)
        {
            _activePunches = new List<ActivePunch>();
            return;
        }

        foreach (ActivePunch punch in _activePunches)
        {
            if (punch != null && punch.DamageZone != null)
                punch.DamageZone.Cancel();
        }

        _activePunches.Clear();
    }

    private void ResetRuntimeState()
    {
        if (_setupSequence.isAlive)
            _setupSequence.Stop();

        if (_recoverySequence.isAlive)
            _recoverySequence.Stop();

        if (_currentEnemy != null && _currentEnemy.damageable != null)
            _currentEnemy.damageable.OnTakeDamage.RemoveListener(HandleEnemyHit);

        CancelActivePunches();

        _setupSequence = default;
        _recoverySequence = default;
        _currentEnemy = null;
        _currentExecution = null;
        _player = null;
        _nextPunchTimestamp = 0.0f;
        _launchedPunchCount = 0;
        _isBarrageActive = false;
        _isStopping = false;
    }

    private static Vector3 ToWorldPosition(Vector2 position, float height)
    {
        return new Vector3(position.x, height, position.y);
    }

    private static void PlayAnimation(EnemyController enemy, string animationName)
    {
        if (enemy != null && enemy.animator != null && !string.IsNullOrWhiteSpace(animationName))
            enemy.animator.Play(animationName);
    }
}
