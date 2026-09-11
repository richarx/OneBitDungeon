using System;
using System.Collections.Generic;
using Player.Scripts;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Tutorials
{
    public sealed class TutorialSignalBridge : MonoBehaviour
    {

        public static TutorialSignalBridge Instance { get; private set; }

        [TitleGroup("References")]
        [SerializeField, Required]
        private PlayerStateMachine _player;

        private UnityEvent<TutorialSignal> SignalPublished = new UnityEvent<TutorialSignal>();

        private List<UnityAction<TutorialSignal>> _subscribers = new List<UnityAction<TutorialSignal>>();

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
                SignalPublished.RemoveListener(action);
            _subscribers.Clear();
        }

        private void Start()
        {
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
            if (_isSubscribed || _player == null || _player.playerAttack == null)
                return;

            _player.playerAttack.OnPlayerAttack.AddListener(HandlePlayerAttack);
            _player.playerRoll.OnStartRoll.AddListener(HandlePlayerRoll);
            _player.playerArrogantSpin.OnStartSpin.AddListener(HandlePlayerRoll);
            _isSubscribed = true;
        }

        private void UnsubscribeToActions()
        {
            if (!_isSubscribed || _player == null)
                return;

            _player.playerAttack?.OnPlayerAttack.RemoveListener(HandlePlayerAttack);
            _player.playerRoll?.OnStartRoll.RemoveListener(HandlePlayerRoll);
            _player.playerArrogantSpin?.OnStartSpin.RemoveListener(HandlePlayerRoll);
            _isSubscribed = false;
        }

        public void SubToBridge(UnityAction<TutorialSignal> call)
        {
            if (!_subscribers.Contains(call))
            {
                _subscribers.Add(call);
                SignalPublished.AddListener(call);
            }
        }

        private void HandlePlayerAttack(AttackPayload payload)
        {
            if (payload != null && payload.Type == AttackType.Light)
                SignalPublished?.Invoke(new TutorialSignal(TutorialSignalId.PlayerAttackStarted));
        }

        private void HandlePlayerRoll()
        {
            SignalPublished?.Invoke(new TutorialSignal(TutorialSignalId.PlayerRollStarted));
        }

    }
}
