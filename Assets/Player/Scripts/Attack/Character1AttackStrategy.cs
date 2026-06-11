using Tools_and_Scripts;
using UnityEngine;

namespace Player.Scripts
{
    public class Character1AttackStrategy : IAttackStrategy
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
            // TODO: eventuellement animation différente pour le 2e attack, ou même un 3e attack si on veut faire une combo à 3 coups

            //return attackCount >= 2 ? "Attack_1" : "Attack_2";
            return "Attack_1";
        }

        public bool CanAttack(PlayerStateMachine player)
        {
            return player.playerSword.CurrentlyHasSword && (!player.playerStamina.IsEmpty || player.playerData.canAttackWithNoStamina);
        }

        public void OnTagIn(PlayerStateMachine player)
        {
            player.ChangeBehaviour(player.playerAttack);
        }
    }
}
