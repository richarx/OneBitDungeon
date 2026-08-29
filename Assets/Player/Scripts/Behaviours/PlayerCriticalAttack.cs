using System.Collections.Generic;
using Enemies.Scripts;
using Tools_and_Scripts;
using UnityEngine;
using UnityEngine.Events;

namespace Player.Scripts
{
    public class PlayerCriticalAttack : IPlayerBehaviour
    {
        public UnityEvent OnStartDash = new UnityEvent();
        public UnityEvent OnReachedTarget = new UnityEvent();

        private Vector3 dashStartPosition;
        private Vector3 dashTarget;
        private float dashSpeed;
        private float attackStartTimestamp;
        private float reachedTargetTimestamp;
        private bool hasReachedTarget => reachedTargetTimestamp > 0.0f;

        private bool hasHitObstacle;
        private bool hasStartedDash = false;

        private readonly List<Collider> playerColliders = new List<Collider>();
        private readonly List<Collider> targetColliders = new List<Collider>();

        private AttackPayload currentAttackPayload;

        public void StartBehaviour(PlayerStateMachine player, BehaviourType previous)
        {
            if (!CanCriticalAttack(player) || !player.playerArrogance.ConsumeFullArrogance())
            {
                player.ChangeBehaviour(player.playerIdle);
                return;
            }

            attackStartTimestamp = Time.time;
            reachedTargetTimestamp = -1.0f;
            dashStartPosition = player.position;
            hasStartedDash = false;
            hasHitObstacle = false;

            Vector3 direction = player.playerTargeting.directionToTarget.ToVector2().normalized.ToVector3();
            player.SetLastLookDirection(direction.ToVector2());
            dashTarget = ComputeDashTarget(player, direction);
            dashSpeed = ComputeDashSpeed(player);
            IgnoreTargetCollisions(player);

            currentAttackPayload = new AttackPayload("Critical_Attack", AttackType.Critical, player.playerData.insolenceAttackDamage, 1);
            player.playerAttack.OnPlayerAttack?.Invoke(currentAttackPayload);
        }

        public void UpdateBehaviour(PlayerStateMachine player)
        {
            if (!hasStartedDash && Time.time - attackStartTimestamp >= player.playerData.attackDashDelay)
            {
                player.playerAttack.OnSpawnDamageBox?.Invoke();
                OnStartDash?.Invoke();
                hasStartedDash = true;
            }

            if (!hasReachedTarget && CheckIfReachedTarget(player))
            {
                OnReachedTarget?.Invoke();
                reachedTargetTimestamp = Time.time;
            }

            bool hasReachedTargetAndWaited = hasReachedTarget && Time.time - reachedTargetTimestamp >= 0.3f;
            bool waitedTooLong = Time.time - attackStartTimestamp >= 0.8f;

            if (hasReachedTargetAndWaited || waitedTooLong)
            {
                player.playerAttack.OnRemoveDamageBox?.Invoke();
                StopCriticalAttack(player);
            }
        }

        private bool CheckIfReachedTarget(PlayerStateMachine player)
        {
            if (hasHitObstacle)
                return true;

            return Vector3.Distance(dashStartPosition, player.position) >= Vector3.Distance(dashStartPosition, dashTarget) - 0.05f;
        }

        public void FixedUpdateBehaviour(PlayerStateMachine player)
        {
            if (hasStartedDash && !hasReachedTarget)
                DashTowardTarget(player);
            else
                player.moveVelocity = Vector3.zero;

            player.ApplyMovement();
        }

        public bool CanCriticalAttack(PlayerStateMachine player)
        {
            if (!player.playerAttack.CanAttack(player) || !player.playerArrogance.IsFull || !player.playerTargeting.hasTarget)
                return false;

            GameObject target = player.playerTargeting.Target;
            Damageable damageable = target != null ? target.GetComponent<Damageable>() : null;

            return damageable != null
                && !damageable.IsDead
                && player.playerTargeting.targetDistance > 0.01f
                && player.playerTargeting.targetDistance <= player.playerData.insolenceRange;
        }

        private Vector3 ComputeDashTarget(PlayerStateMachine player, Vector3 direction)
        {
            float targetDistance = player.playerTargeting.targetDistance;

            return player.position + direction * (targetDistance + player.playerData.insolencePastTargetDistance);
        }

        private float ComputeDashSpeed(PlayerStateMachine player)
        {
            float dashDistance = Vector3.Distance(player.position, dashTarget);
            float dashDuration = Mathf.Max(player.playerData.attackDashDuration, Time.fixedDeltaTime);

            return dashDistance / dashDuration;
        }
        private void DashTowardTarget(PlayerStateMachine player)
        {
            Vector3 position = player.position;
            Vector3 toTarget = dashTarget - position;
            float remainingDistance = toTarget.magnitude;

            float stepDistance = dashSpeed * Time.fixedDeltaTime;
            Vector3 direction = toTarget / remainingDistance;
            float castDistance = Mathf.Min(stepDistance + 0.05f, remainingDistance);

            if (Physics.Raycast(position + Vector3.up * 0.5f, direction, castDistance, player.obstaclesLayer))
            {
                hasHitObstacle = true;
                player.moveVelocity = Vector3.zero;
                return;
            }

            player.rb.MovePosition(Vector3.MoveTowards(position, dashTarget, stepDistance));
            player.moveVelocity = Vector3.zero;
        }

        private void StopCriticalAttack(PlayerStateMachine player)
        {
            if (player.moveInput.magnitude >= 0.15f)
                player.ChangeBehaviour(player.inputPackage.GetArroganceMode.isPressed ? player.playerArrogantRun : player.playerRun);
            else
                player.ChangeBehaviour(player.inputPackage.GetArroganceMode.isPressed ? player.playerArrogantIdle : player.playerIdle);
        }

        public void StopBehaviour(PlayerStateMachine player, BehaviourType next)
        {
            player.playerSword.RemoveHitbox();
            player.moveVelocity = Vector3.zero;
            RestoreTargetCollisions();
        }

        public BehaviourType GetBehaviourType()
        {
            return BehaviourType.CriticalAttack;
        }

        private void IgnoreTargetCollisions(PlayerStateMachine player)
        {
            RestoreTargetCollisions();

            playerColliders.AddRange(player.GetComponentsInChildren<Collider>());
            targetColliders.AddRange(player.playerTargeting.Target.GetComponentsInChildren<Collider>());

            foreach (Collider playerCollider in playerColliders)
            {
                foreach (Collider targetCollider in targetColliders)
                {
                    if (playerCollider != null && targetCollider != null)
                        Physics.IgnoreCollision(playerCollider, targetCollider, true);
                }
            }
        }

        private void RestoreTargetCollisions()
        {
            foreach (Collider playerCollider in playerColliders)
            {
                foreach (Collider targetCollider in targetColliders)
                {
                    if (playerCollider != null && targetCollider != null)
                        Physics.IgnoreCollision(playerCollider, targetCollider, false);
                }
            }

            playerColliders.Clear();
            targetColliders.Clear();
        }
    }
}
