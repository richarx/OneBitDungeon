using System;
using Game_Manager;
using SFX;
using Tools_and_Scripts;
using UnityEngine;
using UnityEngine.Assertions;

namespace Decor.Door
{
    public class DoorController : MonoBehaviour
    {
        public enum DoorSide
        {
            North,
            East,
            South,
            West
        }

        [SerializeField] private DoorSide doorSide;
        [SerializeField] private SceneField targetScene;
        [SerializeField] private Animator animator;
        [SerializeField] private DoorTrigger trigger;
        [SerializeField] private GameObject hitbox;

        private bool isLocked;

        public DoorSide doorDirection => doorSide;
        
        private void Start()
        {
            Assert.IsNotNull(targetScene, $"In Door : {doorSide} => target scene has not been set");
            
            trigger.OnTrigger.AddListener(() => GameManager.instance.ChangeScene(targetScene.SceneName, doorSide.Opposite()));
            GameManager.OnLockLevel.AddListener(LockDoor);
            GameManager.OnUnlockLevel.AddListener(UnlockDoor);
        }

        private void UnlockDoor()
        {
            animator.Play("Unlock");
            isLocked = false;
            hitbox.SetActive(isLocked);
            trigger.gameObject.SetActive(!isLocked);
        }

        private void LockDoor()
        {
            animator.Play("Lock");
            isLocked = true;
            hitbox.SetActive(isLocked);
            trigger.gameObject.SetActive(!isLocked);
        }

        public void OpenForEnemy()
        {
            animator.Play("QuickOpen");
        }

        public Vector3 ComputeSpawnPosition()
        {
            Vector3 position = transform.position;
            float distance = 0.1f;

            switch (doorSide)
            {
                case DoorSide.North:
                    position += Vector3.forward * distance;
                    break;
                case DoorSide.East:
                    position += Vector3.right * distance;
                    break;
                case DoorSide.South:
                    position -= Vector3.forward * distance;
                    break;
                case DoorSide.West:
                    position -= Vector3.right * distance;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return position;
        }
    }
}
