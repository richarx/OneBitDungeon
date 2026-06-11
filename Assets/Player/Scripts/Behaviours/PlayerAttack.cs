using Player.Sword_Hitboxes;
using Tools_and_Scripts;
using UnityEngine;
using UnityEngine.Events;
using static CodeAnimator;

namespace Player.Scripts
{
    public class PlayerAttack : IPlayerBehaviour
    {
        public UnityEvent<string> OnPlayerAttack = new UnityEvent<string>();
        public UnityEvent OnSpawnDamageBox = new UnityEvent();
        public UnityEvent OnRemoveDamageBox = new UnityEvent();

        private Vector3 dashTarget;
        private float attackStartTimestamp;

        private Vector3 dashVelocity;
        private bool hasHitObstacle;

        private int attackCount;
        public bool IsSecondAttack => attackCount % 2 == 0;
        public int AttackCount => attackCount;
        private bool canAttackBeCanceled;
        private bool hasSpawnedDamageBox;
        private bool hasRemovedDamageBox;

        private IAttackStrategy currentStrategy;

        public PlayerAttack(PlayerStateMachine player, IAttackStrategy strategy)
        {
            currentStrategy = strategy;
            currentStrategy.Initialize(player);
        }

        public void SetStrategy(IAttackStrategy strategy)
        {
            currentStrategy = strategy;
        }

        public void TriggerTagIn(PlayerStateMachine player)
        {
            currentStrategy.OnTagIn(player);
        }

        public void StartBehaviour(PlayerStateMachine player, BehaviourType previous)
        {
            attackStartTimestamp = Time.time;
            canAttackBeCanceled = false;
            hasSpawnedDamageBox = false;
            hasRemovedDamageBox = false;
            hasHitObstacle = false;
            attackCount = previous == BehaviourType.Attack ? attackCount + 1 : 1;

            ComputeDashTarget(player);
            player.SetLastLookDirection((dashTarget - player.position).ToVector2());

            player.playerStamina.ConsumeStamina(player.playerData.attackStaminaCost);

            OnPlayerAttack?.Invoke(SelectCurrentAttack());
        }

        public void UpdateBehaviour(PlayerStateMachine player)
        {
            if (Time.time - attackStartTimestamp >= player.playerData.attackDuration)
            {
                player.ChangeBehaviour(player.playerIdle);
                return;
            }

            if (!canAttackBeCanceled && Time.time - attackStartTimestamp >= player.playerData.attackCancelTimer)
            {
                canAttackBeCanceled = true;
            }

            if (!hasSpawnedDamageBox && Time.time - attackStartTimestamp >= player.playerData.attackSpawnHitBoxTimer)
            {
                hasSpawnedDamageBox = true;
                OnSpawnDamageBox?.Invoke();
            }

            if (!hasRemovedDamageBox && Time.time - attackStartTimestamp >= player.playerData.attackRemoveHitBoxTimer)
            {
                hasRemovedDamageBox = true;
                OnRemoveDamageBox?.Invoke();
            }

            if (canAttackBeCanceled && attackCount < player.playerData.maxAttackCountInCombo && CanAttack(player) && player.inputPackage.GetAttack.WasPressedWithBuffer())
            {
                StartBehaviour(player, BehaviourType.Attack);
                return;
            }

            if (canAttackBeCanceled && player.playerTagSystem != null && attackCount < player.playerData.maxAttackCountInCombo && CanAttack(player) && player.playerTagSystem.CanTag && player.inputPackage.GetTag.WasPressedWithBuffer())
            {
                player.ChangeBehaviour(player.playerTag);
                return;
            }

            if (canAttackBeCanceled && player.playerRoll.CanRoll(player) && player.inputPackage.GetRoll.WasPressedWithBuffer())
            {
                player.ChangeBehaviour(player.playerRoll);
                return;
            }

            if (canAttackBeCanceled && player.playerJump.CanJump(player) && player.inputPackage.GetJump.WasPressedWithBuffer())
            {
                player.ChangeBehaviour(player.playerJump);
                return;
            }

            if (canAttackBeCanceled && player.playerParry.CanParry(player) && player.inputPackage.GetParry.WasPressedWithBuffer())
            {
                player.ChangeBehaviour(player.playerParry);
                return;
            }
        }

        public void FixedUpdateBehaviour(PlayerStateMachine player)
        {
            if (!hasHitObstacle && Time.time - attackStartTimestamp <= player.playerData.attackDashDuration && Vector3.Distance(player.position, dashTarget) >= 0.1f)
            {
                CheckForObstacle(player);
                DashTowardTarget(player);
            }
            else
                player.moveVelocity = Vector3.zero;

            player.ApplyMovement();
        }

        private void CheckForObstacle(PlayerStateMachine player)
        {
            Vector3 position = player.position + Vector3.up * 0.5f;
            Vector3 direction = (dashTarget - player.position).normalized;

            bool hasHit = Physics.Raycast(position, direction, 1.0f, player.obstaclesLayer);

            if (hasHit)
                hasHitObstacle = true;
        }

        private void ComputeDashTarget(PlayerStateMachine player)
        {
            dashTarget = currentStrategy.ComputeDashTarget(player);
        }

        private string SelectCurrentAttack()
        {
            return currentStrategy.SelectAttackName(attackCount);
        }

        private void DashTowardTarget(PlayerStateMachine player)
        {
            if (hasHitObstacle)
                return;

            player.rb.MovePosition(Vector3.SmoothDamp(player.position, dashTarget, ref dashVelocity, player.playerData.attackDashDuration));
        }

        public void StopBehaviour(PlayerStateMachine player, BehaviourType next)
        {
            player.playerSword.RemoveHitbox();
        }

        public bool CanAttack(PlayerStateMachine player)
        {
            return currentStrategy.CanAttack(player);
        }

        private Vector2 ComputeDashDirection(PlayerStateMachine player)
        {
            return (dashTarget - player.position).ToVector2().normalized;
        }

        public BehaviourType GetBehaviourType()
        {
            return BehaviourType.Attack;
        }
    }
}
