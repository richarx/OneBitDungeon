using System;
using Game_Manager;
using UnityEngine;

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
        [SerializeField] private Animator animator;

        private bool isLocked;
        
        private void Start()
        {
            GameManager.OnLockLevel.AddListener(LockDoor);
            GameManager.OnUnlockLevel.AddListener(UnlockDoor);
        }

        private void UnlockDoor()
        {
            animator.Play("Unlock");
            isLocked = false;
        }

        private void LockDoor()
        {
            animator.Play("Lock");
            isLocked = true;
        }

        public void OpenForEnemy()
        {
            animator.Play("QuickOpen");
        }

        public Vector3 ComputeSpawnPosition()
        {
            Vector3 position = transform.position;

            switch (doorSide)
            {
                case DoorSide.North:
                    position += Vector3.forward * 0.3f;
                    break;
                case DoorSide.East:
                    position += Vector3.right * 0.3f;
                    break;
                case DoorSide.South:
                    position -= Vector3.forward * 0.3f;
                    break;
                case DoorSide.West:
                    position -= Vector3.right * 0.3f;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return position;
        }
    }
}
