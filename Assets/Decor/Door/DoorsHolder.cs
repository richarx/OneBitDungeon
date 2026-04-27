using System.Collections.Generic;
using System.Linq;
using Game_Manager;
using SFX;
using UnityEngine;
using static Decor.Door.DoorController;
using Random = UnityEngine.Random;

namespace Decor.Door
{
    public class DoorsHolder : MonoBehaviour
    {
        [SerializeField] private bool isRoomLocking;

        [Space]
        [SerializeField] private AudioClip openSound;
        [SerializeField] private AudioClip closeSound;

        private List<DoorController> doors;
        public static DoorsHolder instance;

        private void Awake()
        {
            instance = this;

            GameManager.OnLockLevel.AddListener(LockLevel);
            GameManager.OnUnlockLevel.AddListener(UnLockLevel);
            SetupDoors();
        }

        private void UnLockLevel()
        {
            if (!isRoomLocking)
                return;

            SFXManager.instance.PlaySFX(openSound, 0.1f, delay: 2.0f);

            foreach (DoorController door in doors)
            {
                door.UnlockDoor();
            }
        }

        private void LockLevel()
        {
            if (!isRoomLocking)
                return;

            SFXManager.instance.PlaySFX(closeSound, 0.1f);

            foreach (DoorController door in doors)
            {
                door.LockDoor();
            }
        }

        private void SetupDoors()
        {
            doors = new List<DoorController>();
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).gameObject.activeSelf)
                    doors.Add(transform.GetChild(i).GetComponent<DoorController>());
            }
        }

        public DoorController GetDoor(DoorSide direction)
        {
            DoorController door = doors.First(d => d.doorDirection == direction);

            if (door == null)
                Debug.LogError($"Error in DoorHolder : no door of direction {direction} found.");

            return door;
        }

        public DoorController GetRandomDoor()
        {
            int index = Random.Range(0, doors.Count);
            return doors[index];
        }

        public void PlayQuickOpenSound()
        {
            SFXManager.instance.PlaySFX(openSound);
            SFXManager.instance.PlaySFX(closeSound, 0.1f, delay: 1.0f);
        }
    }
}
