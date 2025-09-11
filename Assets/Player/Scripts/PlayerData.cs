using UnityEngine;

namespace Player.Scripts
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/PlayerData")]
    public class PlayerData : ScriptableObject
    {
        [Header("Movement")]
        public float walkMaxSpeed;
        //public float dialogWalkMaxSpeed;
        public float groundAcceleration;
        public float groundDeceleration;
        
        [Header("Roll")]
        public float rollMaxSpeed;
        public float rollMaxDistance;
        public float rollMaxDuration;
        public float rollAcceleration;
        public float rollDeceleration;
        public float rollDecelerationDistanceThreshold;

        [Header("Combat")] 
        public float attackDuration;
        public float attackDashDuration;
        public float attackDashMaxDistance;
        public float attackDashSpeed;
        public float attackDashAcceleration;
        public float attackDashDeceleration;
    }
}
