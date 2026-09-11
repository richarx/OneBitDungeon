using System.Collections.Generic;
using Player.Scripts;
using UnityEngine;
using UnityEngine.Events;

namespace Tutorials
{
    public sealed class TutorialSignalBridge : MonoBehaviour
    {
        public static TutorialSignalBridge Instance { get; private set; }

        private PlayerStateMachine _player;

        private readonly UnityEvent<TutorialSignal> _signalPublished = new UnityEvent<TutorialSignal>();

        private readonly List<UnityAction<TutorialSignal>> _subscribers = new List<UnityAction<TutorialSignal>>();

        private bool _hasStarted;
        private bool _isSubscribed;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("Another instance of TutorialSignalBridge already exists. Destroying this one.");
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;

            foreach (var action in _subscribers)
                _signalPublished.RemoveListener(action);
            _subscribers.Clear();
        }

        private void Start()
        {
            _player = PlayerStateMachine.instance;
            if (_player == null)
            {
                Debug.LogError("PlayerStateMachine instance not found.");
                return;
            }
            _hasStarted = true;
            SubscribeToActions();
        }

        private void OnEnable()
        {
            if (_hasStarted)
                SubscribeToActions();
        }

        private void OnDisable()
        {
            UnsubscribeToActions();
        }

        private void SubscribeToActions()
        {
            if (_isSubscribed || _player == null || _player.playerAttack == null || _player.arroganceProcessor == null)
                return;

            _player.playerAttack.OnPlayerAttack.AddListener(HandlePlayerAttack);
            _player.playerRoll.OnStartRoll.AddListener(HandlePlayerRoll);
            _player.playerArrogantSpin.OnStartSpin.AddListener(HandlePlayerSpin);
            _player.playerParry.OnSuccessfulParry.AddListener(HandleSuccessfulParry);
            _player.playerJump.OnStartJump.AddListener(HandlePlayerJump);
            _player.playerCounterAttack.OnStartCounterAttack.AddListener(HandleParryCounterAttack);
            _player.playerHealth.OnPlayerTakeDamage.AddListener(HandlePlayerTookDamage);
            _player.arroganceProcessor.OnTauntedForOneSecond += HandleOneSecondTaunt;
            _player.arroganceProcessor.OnTauntedInDangerZoneForOneSecond += HandleOneSecondDangerZoneTaunt;
            ArroganceGainEvents.OnGainRequested += HandleArroganceGainRequested;
            _isSubscribed = true;
        }

        private void UnsubscribeToActions()
        {
            if (!_isSubscribed || _player == null)
                return;

            _player.playerAttack?.OnPlayerAttack.RemoveListener(HandlePlayerAttack);
            _player.playerRoll?.OnStartRoll.RemoveListener(HandlePlayerRoll);
            _player.playerArrogantSpin?.OnStartSpin.RemoveListener(HandlePlayerSpin);
            _player.playerParry?.OnSuccessfulParry.RemoveListener(HandleSuccessfulParry);
            _player.playerJump?.OnStartJump.RemoveListener(HandlePlayerJump);
            _player.playerCounterAttack?.OnStartCounterAttack.RemoveListener(HandleParryCounterAttack);
            _player.playerHealth?.OnPlayerTakeDamage.RemoveListener(HandlePlayerTookDamage);
            if (_player.arroganceProcessor != null)
            {
                _player.arroganceProcessor.OnTauntedForOneSecond -= HandleOneSecondTaunt;
                _player.arroganceProcessor.OnTauntedInDangerZoneForOneSecond -= HandleOneSecondDangerZoneTaunt;
            }
            ArroganceGainEvents.OnGainRequested -= HandleArroganceGainRequested;
            _isSubscribed = false;
        }

        public void SubToBridge(UnityAction<TutorialSignal> call)
        {
            if (call == null)
                return;

            if (!_subscribers.Contains(call))
            {
                _subscribers.Add(call);
                _signalPublished.AddListener(call);
            }
        }

        public void UnsubFromBridge(UnityAction<TutorialSignal> call)
        {
            if (call == null || !_subscribers.Remove(call))
                return;

            _signalPublished.RemoveListener(call);
        }

        /// <summary>
        /// Publishes the outcome of a damage-zone collision that a jump actually avoided.
        /// Zone owners call this instead of treating any jump input as a successful dodge.
        /// </summary>
        public void PublishJumpAvoidedAttack()
        {
            PublishSignal(TutorialSignalId.PlayerJumpAvoidedAttack);
        }

        private void HandlePlayerAttack(AttackPayload payload)
        {
            if (payload == null)
                return;

            if (payload.Type == AttackType.Light)
                PublishSignal(TutorialSignalId.PlayerAttackStarted);
            else if (payload.Type == AttackType.Critical)
                PublishSignal(TutorialSignalId.PlayerCriticalAttackStarted);
        }

        private void HandlePlayerRoll()
        {
            PublishSignal(TutorialSignalId.PlayerRollStarted);
        }

        private void HandlePlayerSpin()
        {
            PublishSignal(TutorialSignalId.PlayerSpinStarted);
        }

        private void HandleArroganceGainRequested(ArroganceGainRequest request)
        {
            if (request != null && request.reason == ArroganceGainReason.CloseDodge)
                PublishSignal(TutorialSignalId.PlayerCloseDodgePerformed);
        }

        private void HandleSuccessfulParry()
        {
            PublishSignal(TutorialSignalId.PlayerParrySucceeded);
        }

        private void HandlePlayerJump()
        {
            PublishSignal(TutorialSignalId.PlayerJumpStarted);
        }

        private void HandleParryCounterAttack()
        {
            PublishSignal(TutorialSignalId.PlayerParryCounterAttackStarted);
        }

        private void HandlePlayerTookDamage(Vector3 direction)
        {
            PublishSignal(TutorialSignalId.PlayerTookDamage);
        }

        private void HandleOneSecondTaunt()
        {
            PublishSignal(TutorialSignalId.PlayerTauntedForOneSecond);
        }

        private void HandleOneSecondDangerZoneTaunt()
        {
            PublishSignal(TutorialSignalId.PlayerTauntedInDangerZoneForOneSecond);
        }

        private void PublishSignal(TutorialSignalId signalId)
        {
            _signalPublished?.Invoke(new TutorialSignal(signalId));
        }
    }
}
