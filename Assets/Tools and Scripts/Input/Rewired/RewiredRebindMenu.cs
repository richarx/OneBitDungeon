using System;
using System.Collections;
using Rewired;
using Rewired.UI.ControlMapper;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools_and_Scripts.RewiredInput
{
    /// <summary>Bridges ControlMapper's lifetime to the legacy gameplay input facade.</summary>
    public sealed class RewiredRebindMenu : MonoBehaviour
    {
        [BoxGroup("Rewired")]
        [Required]
        [SerializeField] private ControlMapper _controlMapper;

        [BoxGroup("Rewired")]
        [InfoBox("Désactivez Screen Toggle Action dans le ControlMapper : ce composant gère lui-même l'ouverture et la fermeture afin d'éviter un double basculement.")]
        [SerializeField] private int _playerId = 1;

        [BoxGroup("Rewired")]
        [SerializeField] private string _toggleRebindMenuActionName = "ToggleRebindMenu";

        private Rewired.Player _player;
        private IDisposable _inputSuspension;
        private Coroutine _releaseCoroutine;
        private bool _hasSavedCursorState;
        private bool _savedCursorVisible;
        private CursorLockMode _savedCursorLockState;
        private bool _isDisabling;

        private void OnEnable()
        {
            if (_controlMapper == null) return;
            _player = ReInput.players.GetPlayer(_playerId);
            _controlMapper.ScreenOpenedEvent += HandleScreenOpened;
            _controlMapper.ScreenClosedEvent += HandleScreenClosed;
            if (_controlMapper.isOpen) BeginInputSuspension();
        }

        private void OnDisable()
        {
            _isDisabling = true;
            if (_controlMapper != null)
            {
                _controlMapper.ScreenOpenedEvent -= HandleScreenOpened;
                _controlMapper.ScreenClosedEvent -= HandleScreenClosed;
                if (_controlMapper.isOpen) _controlMapper.Close(true);
            }
            ReleaseImmediately();
            _isDisabling = false;
        }

        private void Update()
        {
            if (_player == null || string.IsNullOrEmpty(_toggleRebindMenuActionName)) return;
            if (!_player.GetButtonDown(_toggleRebindMenuActionName)) return;

            if (_controlMapper.isOpen) Close();
            else Open();
        }

        /// <summary>UnityEvent entry point for the Commandes button.</summary>
        public void Open()
        {
            if (_controlMapper == null) return;
            BeginInputSuspension();
            _controlMapper.Open();
        }

        /// <summary>UnityEvent entry point for a dedicated return button.</summary>
        public void Close()
        {
            if (_controlMapper != null && _controlMapper.isOpen) _controlMapper.Close(true);
        }

        private void HandleScreenOpened()
        {
            BeginInputSuspension();
        }

        private void HandleScreenClosed()
        {
            if (_isDisabling) return;
            if (_releaseCoroutine == null) _releaseCoroutine = StartCoroutine(ReleaseAfterControlsAreReleased());
        }

        private void BeginInputSuspension()
        {
            if (_releaseCoroutine != null)
            {
                StopCoroutine(_releaseCoroutine);
                _releaseCoroutine = null;
            }
            if (!_hasSavedCursorState)
            {
                _savedCursorVisible = Cursor.visible;
                _savedCursorLockState = Cursor.lockState;
                _hasSavedCursorState = true;
            }
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            if (_inputSuspension == null) _inputSuspension = InputPacker.Suspend();
        }

        private IEnumerator ReleaseAfterControlsAreReleased()
        {
            yield return null;
            while (!InputPacker.AreGameplayControlsReleased()) yield return null;
            _releaseCoroutine = null;
            ReleaseImmediately();
        }

        private void ReleaseImmediately()
        {
            if (_releaseCoroutine != null)
            {
                StopCoroutine(_releaseCoroutine);
                _releaseCoroutine = null;
            }
            _inputSuspension?.Dispose();
            _inputSuspension = null;
            if (!_hasSavedCursorState) return;
            Cursor.visible = _savedCursorVisible;
            Cursor.lockState = _savedCursorLockState;
            _hasSavedCursorState = false;
        }
    }
}
