using Enemies.Goon.Sword;
using UnityEngine;

namespace Enemies.Goon
{
    public class GoonSwordDash : IEnemyBehaviour
    {
        
        private GoonSwordAttack goonSwordAttack;
        
        private float startDashTimestamp;
        private float dashCooldownTimestamp;
        
        private Vector3 dashStartPosition;
        private Vector3 dashTarget;
        private Vector3 dashVelocity;

        public GoonSwordDash(GoonSwordAttack _goonSwordAttack)
        {
            goonSwordAttack = _goonSwordAttack;
        }

        public void StartBehaviour(EnemyStateMachine enemy, BehaviourType previous)
        {
            Debug.Log("GOON DASH");

            dashStartPosition = enemy.position;
            ComputeDashTarget(enemy);
            startDashTimestamp = Time.time;
            
            enemy.enemyAnimation.PlayAnimation("Dash_Sword");
        }

        public void UpdateBehaviour(EnemyStateMachine enemy)
        {
            if (Time.time - startDashTimestamp >= goonSwordAttack.dashDuration)
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
            dashTarget = enemy.position + enemy.directionToPlayer * (-1.0f * goonSwordAttack.dashMaxDistance);
            dashTarget.y = 0.0f;
        }
        
        private void DashTowardTarget(EnemyStateMachine enemy)
        {
            enemy.rb.MovePosition(Vector3.SmoothDamp(enemy.position, dashTarget, ref dashVelocity, goonSwordAttack.dashSpeed));
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
