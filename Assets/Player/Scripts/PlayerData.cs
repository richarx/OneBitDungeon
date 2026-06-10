using UnityEngine;

namespace Player.Scripts
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/PlayerData")]
    public class PlayerData : ScriptableObject
    {
        [Header("Movement")]
        public float walkMaxSpeed;
        public float dialogWalkMaxSpeed;
        public float groundAcceleration;
        public float groundDeceleration;

        [Space]
        [Header("Roll")]
        public float rollMaxSpeed;
        public float rollMaxDistance;
        public float rollMaxDuration;
        public float rollAcceleration;
        public float rollDeceleration;
        public float rollDecelerationDistanceThreshold;
        public float rollCooldown;
        public bool rollHasIFrames;

        [Space]
        [Header("Jump")]
        public float jumpMaxSpeed;
        public float jumpMaxDuration;
        public float jumpInAirDuration;
        public float jumpAcceleration;
        public float jumpDeceleration;
        public float jumpCooldown;

        [Space]
        [Header("Combat")]
        public int maxAttackCountInCombo;
        public float attackDuration;
        public float attackCancelTimer;
        public float attackSpawnHitBoxTimer;
        public float attackRemoveHitBoxTimer;
        public float attackDashDuration;
        public float attackDashMaxDistance;
        public float attackDashDeceleration;

        [Space]
        [Header("Stagger")]
        public float staggerDuration;
        public float staggerPower;
        public float staggerDeceleration;
        public float invincibilityDuration;

        [Space]
        [Header("Parry")]
        public float parryDuration;
        public float successfulParryDuration;
        public float parryRecoveryDuration;
        public float successfulParryRecoveryDuration;
        public float parryCooldown;
        public float parryGracePeriodDuration;

        [Space]
        [Header("Stamina")]
        public float maxStamina;
        public float staminaCooldown;
        public float staminaEmptyCooldown;
        public float refillRate;

        [Space]
        [Header("Sit")]
        public float sitDownRotationDampening;
        public float timeInIdleBeforeSitting;

        [Space]
        public float rollStaminaCost;
        public float jumpStaminaCost;
        public float attackStaminaCost;
        public float parryStaminaCost;
        public float parryStaminaGainOnSuccess;


        [Space]
        public bool canAttackWithNoStamina;

        [Space]
        [Header("Tag")]
        public float tagCooldown;
        public float tagDuration;
        public float inactiveHealthRegenRate;
        public float inactiveStaminaRegenRate;
    }
}
