using UnityEngine;

namespace Enemies.Goon
{
    public class EnemyIdle : IEnemyBehaviour
    {
        private float endIdleTimestamp;
        
        public void StartBehaviour(EnemyStateMachine enemy, BehaviourType previous)
        {
            Debug.Log("GOON IDLE");

            endIdleTimestamp = Time.time + Random.Range(0.5f, 1.5f);
        }

        public void UpdateBehaviour(EnemyStateMachine enemy)
        {
            if (Time.time >= endIdleTimestamp)
            {
                enemy.SelectNextBehaviour();
                return;
            }
            
            enemy.ComputeLastLookDirection();
        }

        public void FixedUpdateBehaviour(EnemyStateMachine enemy)
        {
            HandleDirection(enemy);
            enemy.ApplyMovement();
        }
        
        private void HandleDirection(EnemyStateMachine enemy)
        {
            enemy.moveVelocity.x = Mathf.MoveTowards(enemy.moveVelocity.x, 0.0f, enemy.enemyData.groundDeceleration * Time.fixedDeltaTime);
            enemy.moveVelocity.z = Mathf.MoveTowards(enemy.moveVelocity.z, 0.0f, enemy.enemyData.groundDeceleration * Time.fixedDeltaTime);
        }

        public void StopBehaviour(EnemyStateMachine enemy, BehaviourType next)
        {
        }

        public BehaviourType GetBehaviourType()
        {
            return BehaviourType.Idle;
        }
    }
}
