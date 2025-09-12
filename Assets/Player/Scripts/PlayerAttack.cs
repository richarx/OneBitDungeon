using System.Collections.Generic;
using Enemies;
using Tools_and_Scripts;
using UnityEngine;
using UnityEngine.Events;

namespace Player.Scripts
{
    public class PlayerAttack : IPlayerBehaviour
    {
        public UnityEvent<string> OnPlayerAttack = new UnityEvent<string>();

        private Vector3 dashTarget;
        private string currentAttack;
        private float attackStartTimestamp;

        private Vector3 dashVelocity;
        
        public void StartBehaviour(PlayerStateMachine player, BehaviourType previous)
        {
            attackStartTimestamp = Time.time;

            ComputeDashTarget(player);
            SelectCurrentAttack(player);
            player.SetLastLookDirection((dashTarget - player.position).ToVector2());
            
            OnPlayerAttack?.Invoke(currentAttack);
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

        private void SelectCurrentAttack(PlayerStateMachine player)
        {
            currentAttack = "Attack_1";
        }

        public void UpdateBehaviour(PlayerStateMachine player)
        {
            if (Time.time - attackStartTimestamp >= player.playerData.attackDuration)
            {
                player.ChangeBehaviour(player.playerIdle);
                return;
            }
        }

        public void FixedUpdateBehaviour(PlayerStateMachine player)
        {
            if (Time.time - attackStartTimestamp <= player.playerData.attackDashDuration && Vector3.Distance(player.position, dashTarget) >= 0.1f)
                DashTowardTarget(player);
            else
                player.moveVelocity = Vector3.zero;
                //Decelerate(player);

            player.ApplyMovement();
        }

        private void Decelerate(PlayerStateMachine player)
        {
            player.moveVelocity.x = Mathf.MoveTowards(player.moveVelocity.x, 0.0f, player.playerData.attackDashDeceleration * Time.fixedDeltaTime);
            player.moveVelocity.z = Mathf.MoveTowards(player.moveVelocity.z, 0.0f, player.playerData.attackDashDeceleration * Time.fixedDeltaTime);
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
