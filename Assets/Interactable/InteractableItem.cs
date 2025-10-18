using UnityEngine;
using UnityEngine.Events;

namespace Interactable
{
    public class InteractableItem : MonoBehaviour
    {
        [HideInInspector] public UnityEvent OnInteract = new UnityEvent(); 
        
        private PlayerInteraction player;
        
        public float distanceToPlayer => Vector3.Distance(player.position, transform.position);
        
        private void Start()
        {
            player = PlayerInteraction.instance;
            
            DetectPlayerInRange detection = GetComponent<DetectPlayerInRange>();
            detection.OnPlayerEnterRange.AddListener(() => player.TryRegisterNewItem(this));
            detection.OnPlayerExitRange.AddListener(() => player.TryUnregisterItem(this));
        }

        public virtual void Interact()
        {
            OnInteract?.Invoke();
        }
    }
}
