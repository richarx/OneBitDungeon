using UnityEngine;

namespace Enemies.Goon
{
    [CreateAssetMenu(fileName = "GoonData", menuName = "ScriptableObjects/GoonData")]
    public class GoonData : ScriptableObject
    {
        [Header("Movement")]
        public float walkMaxSpeed;
        public float groundAcceleration;
        public float groundDeceleration;
        public float distanceToPlayerWalkThreshold;

        [Space]
        [Header("Stagger")] 
        public float staggerDuration;
        public float stunDuration;
        public float knockbackPower;

        [Space]
        [Header("Attack")] 
        public float attackDuration;
        public float delayBeforeDash;
        public float attackDashDuration;
        public float attackDashMaxDistance;

        [Space] [Header("Dash")] 
        public float dashDuration;
        public float dashMaxDistance;
        public float dashSpeed;
    }
}
