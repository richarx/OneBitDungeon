using UnityEngine;

namespace Enemies.Scripts.Behaviours
{
    public class EnemySpawn : IEnemyBehaviour
    {
        private float spawnTimestamp;
        private float spawnDelay;

        public bool isLocked => Time.time - spawnTimestamp <= spawnDelay;
        
        public void StartBehaviour(EnemyStateMachine enemy, BehaviourType previous)
        {
            Debug.Log("GOON SPAWN");
            
            spawnTimestamp = Time.time;
            spawnDelay = enemy.enemyData.spawnDelay;

            enemy.moveVelocity = Vector3.zero;
            enemy.ApplyMovement();
        }

        public void UpdateBehaviour(EnemyStateMachine enemy)
        {
            if (Time.time - spawnTimestamp >= enemy.enemyData.spawnWalkDuration)
            {
                enemy.ChangeBehaviour(enemy.enemyIdle);
                return;
            }
            
            enemy.ComputeLastLookDirection();
        }

        public void FixedUpdateBehaviour(EnemyStateMachine enemy)
        {
            if (!isLocked)
            {
                HandleDirection(enemy);
                enemy.ApplyMovement();
            }
        }
        
        private void HandleDirection(EnemyStateMachine enemy)
        {
            Vector3 direction = enemy.position.normalized * -1.0f;
            Vector3 move = direction * enemy.enemyData.walkMaxSpeed;
            
            enemy.moveVelocity.x = Mathf.MoveTowards(enemy.moveVelocity.x, move.x, enemy.enemyData.groundAcceleration * Time.fixedDeltaTime);
            enemy.moveVelocity.z = Mathf.MoveTowards(enemy.moveVelocity.z, move.z, enemy.enemyData.groundAcceleration * Time.fixedDeltaTime);
        }

        public void StopBehaviour(EnemyStateMachine enemy, BehaviourType next)
        {
        }

        public BehaviourType GetBehaviourType()
        {
            return BehaviourType.Spawn;
        }
    }
}
