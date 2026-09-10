using System.Collections.Generic;
using Rewired;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools_and_Scripts.RewiredInput
{
    /// <summary>Caches Rewired's player and the action ids used by the legacy input facade.</summary>
    [DefaultExecutionOrder(-1000)]
    public sealed class RewiredInputRuntime : MonoBehaviour
    {
        public static RewiredInputRuntime Instance { get; private set; }
        public bool IsReady => isActiveAndEnabled && _player != null && ReInput.isReady;
        public int PlayerId => _playerId;
        public int ActionCacheGeneration { get; private set; }
        public InputType LastInputType { get; private set; } = InputType.Keyboard;

        [BoxGroup("Rewired")]
        [SerializeField] private int _playerId;

        private readonly Dictionary<string, int> _actionIds = new Dictionary<string, int>();
        private readonly HashSet<string> _unavailableActions = new HashSet<string>();
        private Rewired.Player _player;
        private bool _hasReportedMissingPlayer;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogError("Only one RewiredInputRuntime may be active. Disabling this duplicate component.", this);
                enabled = false;
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ReInput.InitializedEvent += HandleRewiredInitialized;
            if (ReInput.isReady) Initialize();
        }

        private void OnDestroy()
        {
            ReInput.InitializedEvent -= HandleRewiredInitialized;
            if (Instance == this) Instance = null;
        }

        private void OnEnable()
        {
            if (Instance == null) Instance = this;
            if (ReInput.isReady) Initialize();
        }

        private void OnDisable()
        {
            _player = null;
            _actionIds.Clear();
            _unavailableActions.Clear();
        }

        private void HandleRewiredInitialized() { Initialize(); }

        private void Initialize()
        {
            if (!ReInput.isReady) return;
            _player = ReInput.players.GetPlayer(_playerId);
            _actionIds.Clear();
            _unavailableActions.Clear();
            ActionCacheGeneration++;
            if (_player == null)
            {
                if (!_hasReportedMissingPlayer) Debug.LogError($"RewiredInputRuntime could not find Rewired Player {_playerId}. Configure this Player on the Rewired Input Manager.", this);
                _hasReportedMissingPlayer = true;
                return;
            }

            _hasReportedMissingPlayer = false;
        }

        /// <summary>Caches the action ids requested by a client after Rewired is ready.</summary>
        public void CacheActions(IReadOnlyList<string> actionNames)
        {
            if (!IsReady) return;
            for (int i = 0; i < actionNames.Count; i++)
            {
                string actionName = actionNames[i];
                if (_actionIds.ContainsKey(actionName) || _unavailableActions.Contains(actionName)) continue;
                InputAction action = ReInput.mapping.GetAction(actionName);
                if (action == null)
                {
                    _unavailableActions.Add(actionName);
                    Debug.LogError($"RewiredInputRuntime is missing the required Rewired action '{actionName}'.", this);
                    continue;
                }
                _actionIds.Add(actionName, action.id);
            }
        }

        public float GetAxis(string actionName) => TryGetActionId(actionName, out int id) ? _player.GetAxisRaw(id) : 0.0f;
        public bool GetButton(string actionName) => TryGetActionId(actionName, out int id) && _player.GetButton(id);
        public bool GetButtonDown(string actionName) => TryGetActionId(actionName, out int id) && _player.GetButtonDown(id);

        public InputType GetLastInputType()
        {
            if (!IsReady) return LastInputType;
            Controller controller = _player.controllers.GetLastActiveController();
            if (controller != null) LastInputType = controller.type == ControllerType.Joystick ? InputType.Gamepad : InputType.Keyboard;
            return LastInputType;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            Instance = null;
        }

        private bool TryGetActionId(string actionName, out int actionId)
        {
            actionId = -1;
            return IsReady && _actionIds.TryGetValue(actionName, out actionId);
        }
    }
}
