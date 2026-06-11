using System;
using Tools_and_Scripts;
using UnityEngine;

namespace Player.Scripts
{
    public class Character2AttackStrategy : IAttackStrategy
    {
        public void Initialize(PlayerStateMachine player) { }

        public Vector3 ComputeDashTarget(PlayerStateMachine player, AttackPayload attackPayload)
        {
            if (attackPayload != null && (attackPayload.Type == AttackType.Special || attackPayload.Type == AttackType.Punish))
                return player.position; // No dash for special and punish attacks

            bool hasTarget = player.playerTargeting.hasTarget;
            bool isTargetInRange = hasTarget && player.playerTargeting.targetDistance <= player.playerData.attackDashMaxDistance;
            bool isInputPressed = player.moveInput.magnitude >= 0.15f;
            bool isInputInTargetDirection = hasTarget && isInputPressed && Vector3.Dot(player.playerTargeting.directionToTarget, player.moveInput.ToVector3()) >= 0.5f;

            if (isTargetInRange && (!isInputPressed || isInputInTargetDirection))
                return player.position + (player.playerTargeting.directionToTarget * Mathf.Max(player.playerTargeting.targetDistance - 1.0f, 0.0f));
            else if (isInputPressed)
                return player.position + player.moveInput.ToVector3().normalized * player.playerData.attackDashMaxDistance;
            else
                return player.position + player.LastLookDirection.ToVector3().normalized * player.playerData.attackDashMaxDistance;
        }

        public AttackPayload SelectAttackPayload(int attackCount, TagContext tagContext)
        {
            if (tagContext == TagContext.Attack)
                return new AttackPayload("Sword_Whirlwind", AttackType.Special, attackCount, tagContext);
            else if (tagContext == TagContext.SucceededParry)
                return new AttackPayload("Counter_Attack", AttackType.Punish, attackCount, tagContext);

            return new AttackPayload("Attack_1", AttackType.Light, attackCount, tagContext);
        }

        public void OnAttackStart(PlayerStateMachine player, AttackPayload attackPayload)
        {
            if (attackPayload != null && (attackPayload.Type == AttackType.Punish || attackPayload.Type == AttackType.Special))
            {
                // Tp here if it's a special or punish attack
            }

        }

        public bool CanAttack(PlayerStateMachine player)
        {
            return player.playerSword.CurrentlyHasSword && (!player.playerStamina.IsEmpty || player.playerData.canAttackWithNoStamina);
        }

        public void OnTagAttack() { }
    }
}
