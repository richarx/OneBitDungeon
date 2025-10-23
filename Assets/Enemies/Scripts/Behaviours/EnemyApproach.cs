using UnityEngine;

namespace Enemies.Scripts.Behaviours
{
    public class EnemyApproach : IEnemyBehaviour
    {
        public void StartBehaviour(EnemyStateMachine enemy, BehaviourType previous)
        {            
            Debug.Log("GOON APPROACH");
        }

        public void UpdateBehaviour(EnemyStateMachine enemy)
        {
            if (enemy.distanceToPlayer <= enemy.enemyData.distanceToPlayerApproachThreshold)
            {
                enemy.ChangeBehaviour(enemy.enemyAttack);
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
            Vector3 direction = enemy.directionToPlayer;
            Vector3 move = direction * enemy.enemyData.walkMaxSpeed;
            
            enemy.moveVelocity.x = Mathf.MoveTowards(enemy.moveVelocity.x, move.x, enemy.enemyData.groundAcceleration * Time.fixedDeltaTime);
            enemy.moveVelocity.z = Mathf.MoveTowards(enemy.moveVelocity.z, move.z, enemy.enemyData.groundAcceleration * Time.fixedDeltaTime);
        }

        public void StopBehaviour(EnemyStateMachine enemy, BehaviourType next)
        {
        }

        public BehaviourType GetBehaviourType()
        {
            return BehaviourType.Walk;
        }
    }
}
