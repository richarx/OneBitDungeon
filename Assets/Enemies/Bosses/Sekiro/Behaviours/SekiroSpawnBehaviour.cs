using System;
using Enemies.Scripts.Behaviours;
using PrimeTween;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

[Serializable]
public sealed class SekiroSpawnBehaviour : IEnemyBehaviour
{
    // CircleDamageZone turns on collision after its short filled-color transition.
    private const float SpawnCircleImpactTransitionDuration = 0.05f;

    [OdinSerialize, Required, LabelText("Data")]
    private SekiroSpawnData _data;

    [NonSerialized] private Sequence _sequence;
    [NonSerialized] private SpriteRenderer _sprite;
    [NonSerialized] private SpriteRenderer _shadowSprite;
    [NonSerialized] private Rigidbody _rigidbody;
    [NonSerialized] private SphereCollider _hitbox;
    [NonSerialized] private CircleDamageZone _spawnCircle;
    [NonSerialized] private Vector3 _groundPosition;
    [NonSerialized] private bool _spriteWasEnabled;
    [NonSerialized] private bool _shadowWasEnabled;
    [NonSerialized] private bool _disabledHitboxForIntro;
    [NonSerialized] private EnemyController _enemy;
    [NonSerialized] private BehaviourExecution _execution;

    public void StartBehaviour(EnemyController enemy, BehaviourExecution execution)
    {
        ResetRuntimeState(false);
        _enemy = enemy;
        _execution = execution;

        if (_data == null || enemy == null)
        {
            execution.Complete();
            return;
        }

        _sprite = enemy.Sprite;
        _shadowSprite = enemy.shadowSprite;
        _rigidbody = enemy.GetComponent<Rigidbody>();
        _hitbox = enemy.GetComponent<SphereCollider>();
        _groundPosition = _rigidbody != null ? _rigidbody.position : enemy.transform.position;
        _spriteWasEnabled = _sprite != null && _sprite.enabled;
        _shadowWasEnabled = _shadowSprite != null && _shadowSprite.enabled;

        if (_data.DisableHitboxDuringIntro)
        {
            enemy.DeactivateHitbox();
            _disabledHitboxForIntro = true;
        }

        if (_data.HideSpriteDuringDelay)
            SetVisible(false);

        if (_data.UseVerticalEntrance && _rigidbody != null)
            _rigidbody.MovePosition(_groundPosition + Vector3.up * _data.EntranceHeight);

        _sequence = Sequence.Create()
            .ChainDelay(_data.HiddenDelay)
            .ChainCallback(() =>
            {
                SetVisible(true);
                PlayAnimation(enemy, _data.SpawnAnimation);
                SpawnCircle();
            })
            .ChainDelay(GetFallStartDelay())
            .ChainCallback(() => PlayAnimation(enemy, _data.FallAnimation))
            .Chain(_data.UseVerticalEntrance && _rigidbody != null
                ? Tween.RigidbodyMovePosition(_rigidbody, _groundPosition, _data.FallDuration)
                : Tween.Delay(0.0f))
            .ChainCallback(() => PlayAnimation(enemy, _data.ImpactAnimation))
            .ChainCallback(() => RestoreHitbox(enemy))
            .ChainCallback(() => PlayAnimation(enemy, _data.RecoveryAnimation))
            .ChainDelay(_data.RecoveryDuration)
            .ChainCallback(execution.Complete);
    }

    public void UpdateBehaviour(EnemyController enemy)
    {
    }

    public void FixedUpdateBehaviour(EnemyController enemy)
    {
    }

    public void StopBehaviour(EnemyController enemy) => ResetRuntimeState(true);

    public void CancelBehaviour(EnemyController enemy) => ResetRuntimeState(false);

    public void SetSubBehaviourState(bool state)
    {
    }

    private void ResetRuntimeState(bool completedNormally)
    {
        if (_sequence.isAlive)
            _sequence.Stop();

        if (_rigidbody != null)
            _rigidbody.MovePosition(_groundPosition);

        if (_spawnCircle != null && !_spawnCircle.IsDestroyed)
            _spawnCircle.Cancel();

        if (completedNormally)
        {
            SetVisible(true, true);
            RestoreHitbox(_enemy);
        }
        else
        {
            SetVisible(_spriteWasEnabled, _shadowWasEnabled);
            RestoreHitboxSnapshot();
        }
        _sequence = default;
        _sprite = null;
        _shadowSprite = null;
        _rigidbody = null;
        _hitbox = null;
        _spawnCircle = null;
        _enemy = null;
        _execution = null;
        _spriteWasEnabled = false;
        _shadowWasEnabled = false;
        _disabledHitboxForIntro = false;
    }

    private void SetVisible(bool visible)
    {
        SetVisible(visible, visible);
    }

    private void SetVisible(bool spriteVisible, bool shadowVisible)
    {
        if (_sprite != null)
            _sprite.enabled = spriteVisible;

        if (_shadowSprite != null)
            _shadowSprite.enabled = shadowVisible;
    }

    private static void RestoreHitbox(EnemyController enemy)
    {
        if (enemy != null && enemy.isActiveAndEnabled && enemy.damageable != null && !enemy.damageable.IsDead)
            enemy.ActivateHitbox();
    }

    private void SpawnCircle()
    {
        if (!_data.SpawnCircleEnabled || _data.SpawnCirclePrefab == null || _enemy == null)
            return;

        _spawnCircle = UnityEngine.Object.Instantiate(_data.SpawnCirclePrefab, _groundPosition, Quaternion.Euler(90.0f, 0.0f, 0.0f));
        _spawnCircle.Setup(_data.SpawnCircleRadius, _data.SpawnCircleDuration, _data.SpawnCircleFillDuration);
    }

    private float GetSpawnCircleDuration()
    {
        return _data.SpawnCircleEnabled && _data.SpawnCirclePrefab != null
            ? _data.SpawnCircleDuration + _data.SpawnCircleFillDuration + SpawnCircleImpactTransitionDuration
            : 0.0f;
    }

    private float GetFallStartDelay()
    {
        // A damaging circle resolves at Spawn + Fill. When it is enabled, landing
        // exactly there prevents it from hitting before the visible impact.
        float totalIntroDuration = _data.SpawnCircleEnabled && _data.SpawnCirclePrefab != null
            ? GetSpawnCircleDuration()
            : _data.IntroDuration;
        return _data.UseVerticalEntrance && _rigidbody != null
            ? Mathf.Max(0.0f, totalIntroDuration - _data.FallDuration)
            : totalIntroDuration;
    }

    private void RestoreHitboxSnapshot()
    {
        if (_enemy == null || !_enemy.isActiveAndEnabled || _enemy.damageable == null || _enemy.damageable.IsDead)
            return;

        // A cancelled transition must not restore combat while a death or phase
        // transition is taking ownership. The next valid state activates it.
        if (_disabledHitboxForIntro && _hitbox != null)
            _hitbox.enabled = false;
    }

    private static void PlayAnimation(EnemyController enemy, string animationName) => SekiroAnimation.Play(enemy, animationName);
}
