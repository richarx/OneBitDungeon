using Tools_and_Scripts;
using UnityEngine;

namespace Enemies.Scripts.Behaviours
{
    public class EnemyStrafe : IEnemyBehaviour
    {
        private bool isGoingLeft;
        private float endStrafeTimestamp;
        
        public void StartBehaviour(EnemyStateMachine enemy, BehaviourType previous)
        {
            isGoingLeft = Tools.RandomBool();
            endStrafeTimestamp = Time.time + Random.Range(0.5f, 1.5f);
        }

        public void UpdateBehaviour(EnemyStateMachine enemy)
        {
            if (Time.time >= endStrafeTimestamp)
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
            Vector3 direction = enemy.directionToPlayer.ToVector2().AddAngleToDirection(isGoingLeft ? 90.0f : -90.0f).ToVector3();
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
