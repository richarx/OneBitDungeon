using Tools_and_Scripts;
using UnityEngine;
using UnityEngine.Events;

namespace Player.Scripts
{
    public class PlayerAttack : IPlayerBehaviour
    {
        public UnityEvent<string> OnPlayerAttack = new UnityEvent<string>();

        private Vector3 dashTarget;
        private float attackStartTimestamp;

        private Vector3 dashVelocity;

        private bool isSecondAttack;
        public bool IsSecondAttack => isSecondAttack;
        private bool canAttackBeCanceled;

        public PlayerAttack(PlayerStateMachine player)
        {
            player.playerSword.weaponAnimationTriggers.OnAttackCanBeCanceled.AddListener(() => canAttackBeCanceled = true);
        }
        
        public void StartBehaviour(PlayerStateMachine player, BehaviourType previous)
        {
            attackStartTimestamp = Time.time;
            canAttackBeCanceled = false;
            isSecondAttack = previous == BehaviourType.Attack;

            ComputeDashTarget(player);
            player.SetLastLookDirection((dashTarget - player.position).ToVector2());
            
            OnPlayerAttack?.Invoke(SelectCurrentAttack());
        }

        public void UpdateBehaviour(PlayerStateMachine player)
        {
            if (Time.time - attackStartTimestamp >= player.playerData.attackDuration)
            {
                player.ChangeBehaviour(player.playerIdle);
                return;
            }

            if (canAttackBeCanceled && !isSecondAttack && CanAttack(player) && player.inputPackage.GetAttack.WasPressedWithBuffer())
            {
                StartBehaviour(player, BehaviourType.Attack);
                return;
            }
            
            if (canAttackBeCanceled && player.inputPackage.GetRoll.WasPressedWithBuffer())
            {
                player.ChangeBehaviour(player.playerRoll);
                return;
            }
        }

        public void FixedUpdateBehaviour(PlayerStateMachine player)
        {
            if (Time.time - attackStartTimestamp <= player.playerData.attackDashDuration && Vector3.Distance(player.position, dashTarget) >= 0.1f)
                DashTowardTarget(player);
            else
                player.moveVelocity = Vector3.zero;

            player.ApplyMovement();
        }

        private void ComputeDashTarget(PlayerStateMachine player)
        {
            bool hasTarget = player.playerTargeting.hasTarget;
            bool isTargetInRange = hasTarget && player.playerTargeting.targetDistance <= player.playerData.attackDashMaxDistance;
            bool isInputPressed = player.moveInput.magnitude >= 0.15f;
            bool isInputInTargetDirection = hasTarget && isInputPressed && Vector3.Dot(player.playerTargeting.directionToTarget, player.moveInput.ToVector3()) >= 0.5f;
            
            if (isTargetInRange && (!isInputPressed || isInputInTargetDirection))
                dashTarget = player.position + (player.playerTargeting.directionToTarget * Mathf.Max(player.playerTargeting.targetDistance - 1.0f, 0.0f));
            else if (isInputPressed)
                dashTarget = player.position + player.moveInput.ToVector3().normalized * player.playerData.attackDashMaxDistance;
            else
                dashTarget = player.position + player.LastLookDirection.ToVector3().normalized * player.playerData.attackDashMaxDistance;
        }

        private string SelectCurrentAttack()
        {
            return isSecondAttack ? "Attack_1" : "Attack_1";
        }

        private void DashTowardTarget(PlayerStateMachine player)
        {
            player.rb.MovePosition(Vector3.SmoothDamp(player.position, dashTarget, ref dashVelocity, player.playerData.attackDashDuration));
        }

        public void StopBehaviour(PlayerStateMachine player, BehaviourType next)
        {
        }

        public bool CanAttack(PlayerStateMachine player)
        {
            return player.playerSword.CurrentlyHasSword;
        }

        public BehaviourType GetBehaviourType()
        {
            return BehaviourType.Attack;
        }
    }
}
