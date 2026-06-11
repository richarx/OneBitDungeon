using System.Collections.Generic;
using Enemies;
using Enemies.Spawner;
using UnityEngine;

namespace Player.Scripts
{
    public class PlayerTargeting : MonoBehaviour
    {
        private GameObject target;
        public float targetDistance => (targetPosition - transform.position).magnitude;
        public Vector3 targetPosition => target.transform.position;
        public Vector3 directionToTarget => (targetPosition - transform.position).normalized;

        public bool hasTarget => target != null;
        
        public void ComputeTarget(PlayerStateMachine player)
        {
            if (EnemyHolder.instance == null)
            {
                target = null;
                return;
            }
        
            List<GameObject> enemies = EnemyHolder.instance.Enemies;

            Vector3 position = player.position;
            float minDistance = float.MaxValue;
            int closestEnemy = -1;

            for (int i = 0; i < enemies.Count; i++)
            {
                Vector3 directionToEnemy = (enemies[i].transform.position - position);
                float distanceToEnemy = directionToEnemy.magnitude;

                if (distanceToEnemy < minDistance)
                {
                    minDistance = distanceToEnemy;
                    closestEnemy = i;
                }
            }

            if (closestEnemy < 0)
            {
                target = null;
                return;
            }

            target = enemies[closestEnemy];
        }
    }
}
