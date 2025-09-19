using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Decor.Door.DoorController;

namespace Decor.Door
{
    public class DoorsHolder : MonoBehaviour
    {
        [SerializeField] private List<DoorController> doors;

        public static DoorsHolder instance;

        private void Awake()
        {
            instance = this;
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
    }
}
