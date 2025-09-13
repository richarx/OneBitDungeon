using Player.Scripts;
using UnityEngine;
using UnityEngine.Events;

namespace Enemies.Goon
{
    public class GoonSwordAttack : IGoonBehaviour
    {
        public UnityEvent<string> OnGoonSwordAttack = new UnityEvent<string>();

        private Vector3 dashStartPosition;
        private Vector3 dashTarget;
        private float attackStartTimestamp;
        private float attackCooldownTimestamp = -1.0f;

        private bool isAnticipationPhase;
        public bool IsAnticipationPhase => isAnticipationPhase;
        
        private Vector3 dashVelocity;
        
        public void StartBehaviour(GoonStateMachine goon, BehaviourType previous)
        {
            attackStartTimestamp = Time.time;
            isAnticipationPhase = true;
        }

        private void TriggerAttack(GoonStateMachine goon)
        {
            isAnticipationPhase = false;
            dashStartPosition = goon.position;
            ComputeDashTarget(goon);
            OnGoonSwordAttack?.Invoke("Sword");
        }

        public void UpdateBehaviour(GoonStateMachine goon)
        {
            if (isAnticipationPhase && Time.time - attackStartTimestamp >= goon.goonData.delayBeforeDash)
            {
                TriggerAttack(goon);
                return;
            }
            
            if (Time.time - attackStartTimestamp >= goon.goonData.attackDuration)
            {
                goon.ChangeBehaviour(goon.goonIdle);
                return;
            }
            
            if (isAnticipationPhase)
                goon.ComputeLastLookDirection();
        }

        public void FixedUpdateBehaviour(GoonStateMachine goon)
        {
            if (!isAnticipationPhase && Vector3.Distance(dashStartPosition, goon.position) <= Vector3.Distance(dashStartPosition, dashTarget))
                DashTowardTarget(goon);
            else
                goon.moveVelocity = Vector3.zero;

            goon.ApplyMovement();
        }
        
        private void ComputeDashTarget(GoonStateMachine goon)
        {
            dashTarget = goon.position + goon.directionToPlayer * Mathf.Max(goon.distanceToPlayer - 1, 1.0f);
        }
        
        private void DashTowardTarget(GoonStateMachine goon)
        {
            goon.rb.MovePosition(Vector3.SmoothDamp(goon.position, dashTarget, ref dashVelocity, goon.goonData.attackDashDuration));
        }

        public bool CanAttack()
        {
            return attackCooldownTimestamp <= 0.0f || Time.time >= attackCooldownTimestamp;
        }

        public void StopBehaviour(GoonStateMachine goon, BehaviourType next)
        {
            attackCooldownTimestamp = Time.time + Random.Range(1.5f, 3.0f);
        }

        public BehaviourType GetBehaviourType()
        {
            return BehaviourType.Attack;
        }
    }
}
