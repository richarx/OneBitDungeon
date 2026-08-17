using System.Collections.Generic;
using Enemies.Scripts;
using Tools_and_Scripts;
using UnityEngine;
using UnityEngine.Events;

namespace Player.Scripts
{
    public class PlayerCriticalAttack : IPlayerBehaviour
    {

        public UnityEvent<AttackPayload> OnPlayerAttack = new UnityEvent<AttackPayload>();
        public UnityEvent OnSpawnDamageBox = new UnityEvent();
        public UnityEvent OnRemoveDamageBox = new UnityEvent();

        private Vector3 dashTarget;
        private float dashSpeed;
        private float attackStartTimestamp;
        private bool hasRemovedDamageBox;
        private bool hasHitObstacle;
        private readonly List<Collider> playerColliders = new List<Collider>();
        private readonly List<Collider> targetColliders = new List<Collider>();

        public void StartBehaviour(PlayerStateMachine player, BehaviourType previous)
        {
            if (!CanCriticalAttack(player) || !player.playerArrogance.ConsumeFullArrogance())
            {
                player.ChangeBehaviour(player.playerIdle);
                return;
            }

            attackStartTimestamp = Time.time;
            hasRemovedDamageBox = false;
            hasHitObstacle = false;

            Vector3 direction = player.playerTargeting.directionToTarget.ToVector2().normalized.ToVector3();
            player.SetLastLookDirection(direction.ToVector2());
            dashTarget = ComputeDashTarget(player, direction);
            dashSpeed = ComputeDashSpeed(player);
            IgnoreTargetCollisions(player);

            player.playerStamina.ConsumeStamina(player.playerData.attackStaminaCost);
            OnPlayerAttack?.Invoke(new AttackPayload("Critical_Attack", AttackType.Critical, 1));
            OnSpawnDamageBox?.Invoke();
        }

        public void UpdateBehaviour(PlayerStateMachine player)
        {
            float elapsedTime = Time.time - attackStartTimestamp;

            float hitboxRemovalTime = GetHitboxRemovalTime(player);

            if (!hasRemovedDamageBox && elapsedTime >= hitboxRemovalTime)
            {
                hasRemovedDamageBox = true;
                OnRemoveDamageBox?.Invoke();
            }

            if (elapsedTime >= Mathf.Max(player.playerData.attackDuration, hitboxRemovalTime))
                StopCriticalAttack(player);
        }

        public void FixedUpdateBehaviour(PlayerStateMachine player)
        {
            if (!hasHitObstacle && Time.time - attackStartTimestamp <= player.playerData.attackDashDuration)
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

        private float GetHitboxRemovalTime(PlayerStateMachine player)
        {
            return Mathf.Max(player.playerData.attackRemoveHitBoxTimer, player.playerData.attackDashDuration);
        }

        private void DashTowardTarget(PlayerStateMachine player)
        {
            Vector3 position = player.position;
            Vector3 toTarget = dashTarget - position;
            float remainingDistance = toTarget.magnitude;

            if (remainingDistance <= 0.05f)
            {
                player.moveVelocity = Vector3.zero;
                return;
            }

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
