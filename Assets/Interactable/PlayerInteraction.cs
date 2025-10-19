using Player.Scripts;
using UnityEngine;
using UnityEngine.Events;

namespace Interactable
{
    public class PlayerInteraction : MonoBehaviour
    {
        [HideInInspector] public UnityEvent OnUnregisterItem = new UnityEvent();
        [HideInInspector] public UnityEvent OnInteractWithItem = new UnityEvent();

        public static PlayerInteraction instance;
        
        private InteractableItem currentItem;
        public InteractableItem CurrentItem => currentItem;

        public bool IsInteractableInRange => currentItem != null;

        public float CurrentItemDistance => currentItem.distanceToPlayer;

        public Vector3 position => transform.position;
        
        private void Awake()
        {
            instance = this;
        }

        public void InteractWithItem()
        {
            if (!IsInteractableInRange)
                return;
            
            currentItem.Interact();
            OnInteractWithItem?.Invoke();
        }
        
        public void TryRegisterNewItem(InteractableItem newItem)
        {
            if (currentItem == newItem)
                return;
            
            if (currentItem != null && CurrentItemDistance < newItem.distanceToPlayer)
                return;

            currentItem = newItem;
        }

        public void TryUnregisterItem(InteractableItem item)
        {
            if (currentItem == item)
            {
                currentItem = null;
                OnUnregisterItem?.Invoke();
            }
        }
    }
}
