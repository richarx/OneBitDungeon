using UnityEngine;

namespace Player
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/PlayerData")]
    public class PlayerData : ScriptableObject
    {
        [Header("Movement - Ground")]
        public float walkMaxSpeed;
        public float dialogWalkMaxSpeed;
        public float groundAcceleration;
        public float groundDeceleration;
    }
}
