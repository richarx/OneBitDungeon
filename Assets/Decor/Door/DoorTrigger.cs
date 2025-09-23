using Game_Manager;
using UnityEngine;
using UnityEngine.Events;

namespace Decor.Door
{
    public class DoorTrigger : MonoBehaviour
    {
        [HideInInspector] public UnityEvent OnTrigger = new UnityEvent();

        private bool isSetup = false;

        private void Start()
        {
            GameManager.OnChangeScene.AddListener(() => isSetup = true);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isSetup && other.CompareTag("Player"))
                OnTrigger?.Invoke();
        }
    }
}
