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
        
        [Space]
        [Header("Roll")]
        public float rollMaxSpeed;
        public float rollMaxDistance;
        public float rollMaxDuration;
        public float rollAcceleration;
        public float rollDeceleration;
        public float rollDecelerationDistanceThreshold;
        public float rollCooldown;

        [Space]
        [Header("Combat")] 
        public float attackDuration;
        public float attackDashDuration;
        public float attackDashMaxDistance;
        public float attackDashDeceleration;

        [Space]
        [Header("Stagger")] 
        public float staggerDuration;
        public float staggerPower;
        public float staggerDeceleration;
        
        [Space]
        [Header("Parry")] 
        public float parryDuration;
        public float successfulParryDuration;
        public float parryCooldown;
        
        [Space]
        [Header("Stamina")]
        public float maxStamina;
        public float staminaCooldown;
        public float staminaEmptyCooldown;
        public float refillRate;

        [Space] 
        public float rollStaminaCost;
        public float attackStaminaCost;
        public float parryStaminaCost;
    }
}
