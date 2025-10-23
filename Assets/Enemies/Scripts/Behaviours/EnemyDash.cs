using Enemies.Goon.Sword;
using UnityEngine;

namespace Enemies.Scripts.Behaviours
{
    public class EnemyDash : IEnemyBehaviour
    {
        public class EnemyDashData
        {
            public float dashDuration;
            public float dashMaxDistance;
            public float dashSpeed;

            public EnemyDashData(float dashDuration, float dashMaxDistance, float dashSpeed)
            {
                this.dashDuration = dashDuration;
                this.dashMaxDistance = dashMaxDistance;
                this.dashSpeed = dashSpeed;
            }
        }
        private EnemyDashData data;
        
        private float startDashTimestamp;
        private float dashCooldownTimestamp;
        
        private Vector3 dashStartPosition;
        private Vector3 dashTarget;
        private Vector3 dashVelocity;

        public EnemyDash(EnemyDashData enemyDashData)
        {
            data = enemyDashData;
        }

        public void StartBehaviour(EnemyStateMachine enemy, BehaviourType previous)
        {
            dashStartPosition = enemy.position;
            ComputeDashTarget(enemy);
            startDashTimestamp = Time.time;
            
            enemy.enemyAnimation.PlayAnimation("Dash");
        }

        public void UpdateBehaviour(EnemyStateMachine enemy)
        {
            if (Time.time - startDashTimestamp >= data.dashDuration)
            {
                enemy.SelectNextBehaviour();
                return;
            }
            
            enemy.ComputeLastLookDirection();
        }

        public void FixedUpdateBehaviour(EnemyStateMachine enemy)
        {
            if (Vector3.Distance(dashStartPosition, enemy.position) <= Vector3.Distance(dashStartPosition, dashTarget))
                DashTowardTarget(enemy);
            else
                enemy.moveVelocity = Vector3.zero;

            enemy.ApplyMovement();
        }
        
        private void ComputeDashTarget(EnemyStateMachine enemy)
        {
            dashTarget = enemy.position + enemy.directionToPlayer * (-1.0f * data.dashMaxDistance);
            dashTarget.y = 0.0f;
        }
        
        private void DashTowardTarget(EnemyStateMachine enemy)
        {
            enemy.rb.MovePosition(Vector3.SmoothDamp(enemy.position, dashTarget, ref dashVelocity, data.dashSpeed));
        }

        public bool CanDash(EnemyStateMachine enemy)
        {
            return enemy.currentBehaviour.GetBehaviourType() != BehaviourType.Dash && (dashCooldownTimestamp <= 0.0f || Time.time >= dashCooldownTimestamp);
        }

        public void StopBehaviour(EnemyStateMachine enemy, BehaviourType next)
        {
            dashCooldownTimestamp = Time.time + Random.Range(1.5f, 3.0f);
        }

        public BehaviourType GetBehaviourType()
        {
            return BehaviourType.Dash;
        }
    }
}
