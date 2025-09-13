using Player.Scripts;
using Tools_and_Scripts;
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

        private Vector3 dashVelocity;
        
        public void StartBehaviour(GoonStateMachine goon, BehaviourType previous)
        {
            dashStartPosition = goon.position;
            attackStartTimestamp = Time.time;

            ComputeDashTarget(goon);
            goon.SetLastLookDirection((dashTarget - goon.position).ToVector2());
            
            OnGoonSwordAttack?.Invoke("Sword");
        }

        public void UpdateBehaviour(GoonStateMachine goon)
        {
            if (Time.time - attackStartTimestamp >= goon.goonData.attackDuration)
            {
                goon.ChangeBehaviour(goon.goonIdle);
                return;
            }
        }

        public void FixedUpdateBehaviour(GoonStateMachine goon)
        {
            float timeElapsed = Time.time - attackStartTimestamp;
            bool isTimeToDash = timeElapsed >= goon.goonData.delayBeforeDash;

            if (isTimeToDash && Vector3.Distance(dashStartPosition, goon.position) <= Vector3.Distance(dashStartPosition, dashTarget))
            {
                Debug.Log(timeElapsed);
                DashTowardTarget(goon);
            }
            else
                goon.moveVelocity = Vector3.zero;

            goon.ApplyMovement();
        }
        
        private void ComputeDashTarget(GoonStateMachine goon)
        {
            Debug.Log($"Goon position : {goon.position} / Player position : {PlayerStateMachine.instance.position}");
            dashTarget = PlayerStateMachine.instance.position;
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
