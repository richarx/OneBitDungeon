using UnityEngine;
using UnityEngine.Events;

namespace Decor.Door
{
    public class DoorTrigger : MonoBehaviour
    {
        [HideInInspector] public UnityEvent OnTrigger = new UnityEvent();

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
                OnTrigger?.Invoke();
        }
    }
}
