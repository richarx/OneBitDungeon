using Player.Scripts;
using UnityEngine;
using UnityEngine.Events;

namespace Interactable
{
    public class DetectPlayerInRange : MonoBehaviour
    {
        [SerializeField] private float range;

        [HideInInspector] public UnityEvent OnPlayerEnterRange = new UnityEvent();
        [HideInInspector] public UnityEvent OnPlayerExitRange = new UnityEvent();

        private PlayerStateMachine player;
        
        private bool isPlayerInRange;
        public bool IsPlayerInRange => isPlayerInRange;

        private void Start()
        {
            player = PlayerStateMachine.instance;
            player.playerInteraction.OnUnregisterItem.AddListener(() => isPlayerInRange = false);
            player.playerInteraction.OnInteractWithItem.AddListener(() => isPlayerInRange = false);
        }

        private void Update()
        {
            float distance = Vector3.Distance(transform.position, player.position);
            bool isCurrentlyInRange = distance <= range && player.IsAllowedToInteract();

            if (isPlayerInRange != isCurrentlyInRange)
            {
                isPlayerInRange = isCurrentlyInRange;
                
                if (isPlayerInRange)
                    OnPlayerEnterRange?.Invoke();
                else
                    OnPlayerExitRange?.Invoke();
            }
        }
    }
}
