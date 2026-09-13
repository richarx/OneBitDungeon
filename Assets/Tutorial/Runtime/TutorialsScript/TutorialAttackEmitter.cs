using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Tutorials;
using PrimeTween;

public enum TutorialAttackKind
{
    Demonstration,
    Parry,
    Jump
}

[Serializable]
public sealed class TutorialConeAttackProfile
{
    [SerializeField, Required]
    private ConeDamageZone _prefab;

    [SerializeField, MinValue(0)]
    private int _damage = 1;

    [SerializeField, MinValue(0.1f)]
    private float _radius = 5.0f;

    [SerializeField, Range(1.0f, 360.0f)]
    private float _openingAngle = 75.0f;

    [SerializeField, MinValue(0.0f)]
    private float _spawnDuration = 0.25f;

    [SerializeField, MinValue(0.0f)]
    private float _fillDuration = 2.5f;

    [SerializeField, MinValue(0.0f)]
    private float _despawnBuffer = 0.5f;

    [SerializeField]
    private string animation;

    [SerializeField]
    private float animationAnticipationDuration;

    [SerializeField]
    private bool _canBeParried;

    [SerializeField]
    private bool _canBeJumped;

    public bool IsValid => _prefab != null;

    public float Duration => _spawnDuration + _fillDuration + _despawnBuffer;
    public float AnticipationDuration => _spawnDuration + _fillDuration - animationAnticipationDuration;
    public string Animation => animation;

    public ConeDamageZone Spawn(Vector3 origin, Vector3 target)
    {
        if (_prefab == null)
            return null;

        Vector3 flatDirection = target - origin;
        flatDirection.y = 0.0f;
        Vector2 direction = flatDirection.sqrMagnitude <= Mathf.Epsilon
            ? Vector2.down
            : new Vector2(flatDirection.x, flatDirection.z).normalized;

        ConeDamageZone zone = UnityEngine.Object.Instantiate(_prefab, origin, Quaternion.identity);
        DealDamageToPlayer damageDealer = zone.GetComponent<DealDamageToPlayer>();
        if (damageDealer != null)
            damageDealer.Configure(_damage, _canBeParried, _canBeJumped);

        zone.Setup(direction, _radius, _openingAngle, _spawnDuration, _fillDuration);
        return zone;
    }

    public bool CanBeJumped => _canBeJumped;
}

/// <summary>
/// Sole owner of the temporary tutorial attack zones. It creates fixed-direction
/// cones, relays only its own jump outcomes, and can cancel all exercises at once.
/// </summary>
public sealed class TutorialAttackEmitter : MonoBehaviour
{
    [TitleGroup("References")]
    [SerializeField, Required]
    private TutorialSignalBridge _signalBridge;

    [TitleGroup("Profiles")]
    [SerializeField, Required]
    private TutorialConeAttackProfile _demonstration = new TutorialConeAttackProfile();

    [TitleGroup("Profiles")]
    [SerializeField, Required]
    private TutorialConeAttackProfile _parry = new TutorialConeAttackProfile();

    [TitleGroup("Profiles")]
    [SerializeField, Required]
    private TutorialConeAttackProfile _jump = new TutorialConeAttackProfile();

    private readonly List<ActiveAttack> _activeAttacks = new List<ActiveAttack>();

    private Sequence currentAttackAnimation;

    public float GetDuration(TutorialAttackKind kind)
    {
        return GetProfile(kind).Duration;
    }

    public ConeDamageZone Launch(TutorialAttackKind kind, Vector3 origin, Vector3 target)
    {
        RemoveFinishedAttacks();

        TutorialConeAttackProfile profile = GetProfile(kind);
        if (profile == null || !profile.IsValid)
        {
            Debug.LogError($"[{nameof(TutorialAttackEmitter)}] The {kind} attack profile is incomplete.", this);
            return null;
        }

        ConeDamageZone zone = profile.Spawn(origin, target);
        if (zone == null)
            return null;

        DealDamageToPlayer damageDealer = zone.GetComponent<DealDamageToPlayer>();
        Action jumpAvoidedCallback = null;
        if (profile.CanBeJumped && damageDealer != null)
        {
            jumpAvoidedCallback = PublishJumpAvoidedAttack;
            damageDealer.OnPlayerJumpAvoidedAttack += jumpAvoidedCallback;
        }

        if (currentAttackAnimation.isAlive)
            currentAttackAnimation.Stop();

        currentAttackAnimation = Sequence.Create()
            .ChainDelay(profile.AnticipationDuration)
            .ChainCallback(() => GetComponent<EnemyController>().animator.Play(profile.Animation));

        _activeAttacks.Add(new ActiveAttack(zone, damageDealer, jumpAvoidedCallback));
        return zone;
    }

    public void CancelAll()
    {
        if (currentAttackAnimation.isAlive)
            currentAttackAnimation.Stop();

        foreach (ActiveAttack attack in _activeAttacks)
            attack.Cancel();

        _activeAttacks.Clear();
    }

    private void OnDisable()
    {
        CancelAll();
    }

    private void RemoveFinishedAttacks()
    {
        if (currentAttackAnimation.isAlive)
            currentAttackAnimation.Stop();

        for (int index = _activeAttacks.Count - 1; index >= 0; index--)
        {
            ActiveAttack attack = _activeAttacks[index];
            if (!attack.IsFinished)
                continue;

            attack.Detach();
            _activeAttacks.RemoveAt(index);
        }
    }

    private TutorialConeAttackProfile GetProfile(TutorialAttackKind kind)
    {
        switch (kind)
        {
            case TutorialAttackKind.Parry:
                return _parry;
            case TutorialAttackKind.Jump:
                return _jump;
            default:
                return _demonstration;
        }
    }

    private void PublishJumpAvoidedAttack()
    {
        if (_signalBridge != null)
            _signalBridge.PublishJumpAvoidedAttack();
    }

    private sealed class ActiveAttack
    {
        private readonly ConeDamageZone _zone;
        private readonly DealDamageToPlayer _damageDealer;
        private readonly Action _jumpAvoidedCallback;

        public ActiveAttack(ConeDamageZone zone, DealDamageToPlayer damageDealer, Action jumpAvoidedCallback)
        {
            _zone = zone;
            _damageDealer = damageDealer;
            _jumpAvoidedCallback = jumpAvoidedCallback;
        }

        public void Cancel()
        {
            Detach();

            if (_zone != null && !_zone.IsDestroyed)
                _zone.Cancel();
        }

        public bool IsFinished => _zone == null || _zone.IsDestroyed;

        public void Detach()
        {
            if (_damageDealer != null && _jumpAvoidedCallback != null)
                _damageDealer.OnPlayerJumpAvoidedAttack -= _jumpAvoidedCallback;
        }
    }
}
