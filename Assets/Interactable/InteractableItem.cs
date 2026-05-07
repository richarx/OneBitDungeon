using UnityEngine;
using UnityEngine.Events;

namespace Interactable
{
    public class InteractableItem : MonoBehaviour
    {
        [HideInInspector] public UnityEvent OnInteract = new UnityEvent();

        protected PlayerInteraction player;
        protected DetectPlayerInRange detection;
        public bool isBeingUsed { get; protected set; }

        public float distanceToPlayer => Vector3.Distance(player.position, transform.position);

        protected virtual void Start()
        {
            player = PlayerInteraction.instance;

            detection = GetComponent<DetectPlayerInRange>();
            detection.OnPlayerEnterRange.AddListener(() => player.TryRegisterNewItem(this));
            detection.OnPlayerExitRange.AddListener(() => player.TryUnregisterItem(this));
        }

        public virtual void Interact()
        {
            OnInteract?.Invoke();
        }
    }
}
