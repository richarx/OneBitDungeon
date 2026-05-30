using System;
using Tools_and_Scripts;
using UnityEngine;

namespace Player.Scripts
{
    public class Character2AttackStrategy : IAttackStrategy
    {
        public void Initialize(PlayerStateMachine player) { }

        public Vector3 ComputeDashTarget(PlayerStateMachine player)
        {
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

        public string SelectAttackName(int attackCount)
        {
            // TODO: remplacer "Attack_1" par le(s) nom(s) d'animation du perso 2
            return "Attack_1";
        }

        public bool CanAttack(PlayerStateMachine player)
        {
            // TODO: adapter la condition si le perso 2 n'a pas d'épée (magie, etc.)
            return player.playerSword.CurrentlyHasSword && (!player.playerStamina.IsEmpty || player.playerData.canAttackWithNoStamina);
        }

        public void OnTagIn(PlayerStateMachine player) { }
    }
}
