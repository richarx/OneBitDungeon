using Enemies.Goon;
using Enemies.Goon.Sword;
using Tools_and_Scripts;
using UnityEngine;
using Warning_Boxes;

namespace Enemies
{
    public class GoonSwordDashAttack : IEnemyBehaviour
    {
        private GoonSwordAttack goonSwordAttack;
        
        private Vector3 dashStartPosition;
        private Vector3 dashTarget;
        private Vector2 dashDirection => (dashTarget - dashStartPosition).normalized.ToVector2();
        
        private float attackStartTimestamp;
        private float attackCooldownTimestamp = -1.0f;

        private bool isAnticipationPhase;
        public bool IsAnticipationPhase => isAnticipationPhase;
        
        private Vector3 dashVelocity;

        private RectangularWarning warningBox;

        public GoonSwordDashAttack(GoonSwordAttack _goonSwordAttack)
        {
            goonSwordAttack = _goonSwordAttack;
        }

        public void StartBehaviour(EnemyStateMachine enemy, BehaviourType previous)
        {
            attackStartTimestamp = Time.time;
            isAnticipationPhase = true;

            warningBox = WarningBoxes.instance.SpawnRectangularWarning(enemy.position.ToVector2(), enemy.directionToPlayer.ToVector2(), 1.5f, goonSwordAttack.attackDashMaxDistance, goonSwordAttack.delayBeforeDash / 2.0f);
            enemy.OnAttack.Invoke("Anticipation_Sword", Vector2.zero);
        }

        private void TriggerAttack(EnemyStateMachine enemy)
        {
            warningBox.Trigger();
            isAnticipationPhase = false;
            dashStartPosition = enemy.position;
            ComputeDashTarget(enemy);
            enemy.OnAttack?.Invoke("Sword", dashDirection);
        }

        public void UpdateBehaviour(EnemyStateMachine enemy)
        {
            if (isAnticipationPhase && Time.time - attackStartTimestamp >= goonSwordAttack.delayBeforeDash)
            {
                TriggerAttack(enemy);
                return;
            }
            
            if (Time.time - attackStartTimestamp >= goonSwordAttack.attackDuration)
            {
                enemy.ChangeBehaviour(enemy.enemyIdle);
                return;
            }

            if (isAnticipationPhase)
            {
                warningBox.UpdateDirection(enemy.directionToPlayer.ToVector2());
                enemy.ComputeLastLookDirection();
            }
        }

        public void FixedUpdateBehaviour(EnemyStateMachine enemy)
        {
            if (!isAnticipationPhase && Vector3.Distance(dashStartPosition, enemy.position) <= Vector3.Distance(dashStartPosition, dashTarget))
                DashTowardTarget(enemy);
            else
                enemy.moveVelocity = Vector3.zero;

            enemy.ApplyMovement();
        }
        
        private void ComputeDashTarget(EnemyStateMachine enemy)
        {
            dashTarget = enemy.position + enemy.directionToPlayer * Mathf.Max(enemy.distanceToPlayer - 1, 1.0f);
            dashTarget.y = 0.0f;
        }

        private void DashTowardTarget(EnemyStateMachine enemy)
        {
            enemy.rb.MovePosition(Vector3.SmoothDamp(enemy.position, dashTarget, ref dashVelocity, goonSwordAttack.attackDashDuration));
        }

        public bool CanAttack()
        {
            return attackCooldownTimestamp <= 0.0f || Time.time >= attackCooldownTimestamp;
        }

        public void StopBehaviour(EnemyStateMachine enemy, BehaviourType next)
        {
            if (isAnticipationPhase && warningBox != null)
                warningBox.Cancel();
            
            attackCooldownTimestamp = Time.time + Random.Range(1.5f, 3.0f);
            enemy.RemoveDamageHitbox();
        }

        public BehaviourType GetBehaviourType()
        {
            return BehaviourType.Attack;
        }
    }
}
