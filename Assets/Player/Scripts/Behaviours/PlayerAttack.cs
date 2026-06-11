using Player.Sword_Hitboxes;
using Tools_and_Scripts;
using UnityEngine;
using UnityEngine.Events;
using static CodeAnimator;

namespace Player.Scripts
{
    public enum AttackType
    {
        Light,
        Special, 
        Punish
    }

    public class AttackPayload
    {
        public string AttackName { get; private set; }
        public AttackType Type { get; private set; } = AttackType.Light;

        public int AttackCount { get; private set; }
        public TagContext TagContext { get; private set; }

        public AttackPayload(string attackName, AttackType type, int attackCount)
            : this(attackName, type, attackCount, TagContext.None)
        {
        }

        public AttackPayload(string attackName, AttackType type, int attackCount, TagContext tagContext)
        {
            this.AttackName = attackName;
            this.Type = type;
            this.AttackCount = attackCount;
            this.TagContext = tagContext;
        }
    }

    public class PlayerAttack : IPlayerBehaviour
    {
        public UnityEvent<AttackPayload> OnPlayerAttack = new UnityEvent<AttackPayload>();
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
        private AttackPayload currentAttackPayload;

        public PlayerAttack(PlayerStateMachine player, IAttackStrategy strategy)
        {
            currentStrategy = strategy;
            currentStrategy.Initialize(player);
        }

        public void SetStrategy(IAttackStrategy strategy)
        {
            currentStrategy = strategy;
            currentStrategy.Initialize(PlayerStateMachine.instance);
        }

        public void StartBehaviour(PlayerStateMachine player, BehaviourType previous)
        {
            attackStartTimestamp = Time.time;
            canAttackBeCanceled = false;
            hasSpawnedDamageBox = false;
            hasRemovedDamageBox = false;
            hasHitObstacle = false;
            attackCount = previous == BehaviourType.Attack ? attackCount + 1 : 1;

            TagContext tagContext = previous == BehaviourType.Tag ? player.playerTag.TagContext : TagContext.None;
            currentAttackPayload = SelectCurrentAttack(tagContext);
            currentStrategy.OnAttackStart(player, currentAttackPayload);

            ComputeDashTarget(player, currentAttackPayload);
            Vector2 dashDirection = (dashTarget - player.position).ToVector2();
            if (dashDirection.sqrMagnitude > 0.001f)
                player.SetLastLookDirection(dashDirection);

            player.playerStamina.ConsumeStamina(player.playerData.attackStaminaCost);

            OnPlayerAttack?.Invoke(currentAttackPayload);
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

            if (canAttackBeCanceled && player.playerTagSystem != null && CanAttack(player) && player.playerTagSystem.CanTag && player.inputPackage.GetTag.WasPressedWithBuffer())
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

        private void ComputeDashTarget(PlayerStateMachine player, AttackPayload attackPayload)
        {
            dashTarget = currentStrategy.ComputeDashTarget(player, attackPayload);
        }

        private AttackPayload SelectCurrentAttack(TagContext tagContext)
        {
            return currentStrategy.SelectAttackPayload(attackCount, tagContext);
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
