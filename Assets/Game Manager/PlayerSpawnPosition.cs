using UnityEngine;

namespace Game_Manager
{
    public class PlayerSpawnPosition : MonoBehaviour
    {
        public static PlayerSpawnPosition instance;

        private void Awake()
        {
            instance = this;
        }

        public Vector3 GetPosition => transform.position;
    }
}
