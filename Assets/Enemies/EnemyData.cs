using UnityEngine;

namespace Enemies
{
    [CreateAssetMenu(fileName = "EnemyData", menuName = "ScriptableObjects/EnemyData")]
    public class EnemyData : ScriptableObject
    {
        [Header("Spawn")]
        public float spawnDelay;
        public float spawnWalkDuration;

        [Space]
        [Header("Movement")]
        public float walkMaxSpeed;
        public float groundAcceleration;
        public float groundDeceleration;
        public float distanceToPlayerWalkThreshold;
        public float distanceToPlayerApproachThreshold;

        [Space]
        [Header("Stagger")] 
        public float staggerDuration;
        public float stunDuration;
        public float knockbackPower;
    }
}
