using Sirenix.OdinInspector;
using UnityEngine;

namespace Game_Manager
{
    [DisallowMultipleComponent]
    public sealed class ArenaProperty : MonoBehaviour
    {
        public static ArenaProperty Instance { get; private set; }

        [BoxGroup("Player Stagger Bounce")]
        [SerializeField]
        private bool _enablePlayerStaggerBounce = true;

        [BoxGroup("Player Stagger Bounce")]
        [ShowIf(nameof(_enablePlayerStaggerBounce))]
        [LabelText("Speed Retention")]
        [PropertyRange(0.0f, 1.0f)]
        [SerializeField]
        private float _playerStaggerBounceSpeedRetention = 0.75f;

        public bool EnablePlayerStaggerBounce => _enablePlayerStaggerBounce;
        public float PlayerStaggerBounceSpeedRetention => _playerStaggerBounceSpeedRetention;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogError("[ArenaProperty] Plusieurs instances sont présentes dans la scène.", this);
                enabled = false;
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}
