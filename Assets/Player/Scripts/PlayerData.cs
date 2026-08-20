using Sirenix.OdinInspector;
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
        [TitleGroup("Arrogance"), LabelText("Arrogant Walk Max Speed"), MinValue(0.0f)]
        public float arrogantWalkMaxSpeed;

        [TitleGroup("Arrogance"), LabelText("Spin Max Speed"), MinValue(0.0f)]
        public float spinMaxSpeed;

        [TitleGroup("Arrogance"), LabelText("Spin Max Distance"), MinValue(0.0f)]
        public float spinMaxDistance;

        [TitleGroup("Arrogance"), LabelText("Spin Max Duration"), MinValue(0.0f)]
        public float spinMaxDuration;

        [TitleGroup("Arrogance"), LabelText("Spin Acceleration"), MinValue(0.0f)]
        public float spinAcceleration;

        [TitleGroup("Arrogance"), LabelText("Spin Deceleration"), MinValue(0.0f)]
        public float spinDeceleration;

        [TitleGroup("Arrogance"), LabelText("Spin Deceleration Distance Threshold"), MinValue(0.0f)]
        public float spinDecelerationDistanceThreshold;

        [TitleGroup("Arrogance"), LabelText("Spin Cooldown"), MinValue(0.0f)]
        public float spinCooldown;

        [TitleGroup("Arrogance"), LabelText("Close Dodge Window Duration"), SuffixLabel("Seconds"), MinValue(0.0f)]
        public float closeDodgeWindowDuration = 0.25f;


        [TitleGroup("Arrogance Gain"), LabelText("Maximum Arrogance"), MinValue(0.0f)]
        public float maxArrogance = 100.0f;

        [TitleGroup("Arrogance Gain"), LabelText("Arrogance Gained on Close Dodge"), MinValue(0.0f)]
        public float arroganceGainOnCloseDodge = 10.0f;

        [TitleGroup("Arrogance Gain"), LabelText("Arrogance Gained on Parry"), MinValue(0.0f)]
        public float arroganceGainOnParry = 10.0f;

        [TitleGroup("Arrogance Gain"), LabelText("Arrogance Gained on Regualr Attack"), MinValue(0.0f)]
        public float arroganceGainOnAttack = 10.0f;

        [Space]
        [TitleGroup("Arrogance Gain"), LabelText("Use Progressive Arrogance Gain")]
        public bool useProgressiveArroganceGain;

        [TitleGroup("Arrogance Gain"), LabelText("Progressive Arrogance Gain Easing"), ShowIf(nameof(useProgressiveArroganceGain))]
        public ArroganceGainEasing progressiveArroganceGainEasing;

        [TitleGroup("Arrogance Gain"), LabelText("Arrogance State Gain Multiplier"), MinValue(1.0f)]
        public float arroganceStateGainMultiplier = 2.0f;


        [TitleGroup("Insolence"), LabelText("Insolence Attack Damage"), MinValue(0)]
        public int insolenceAttackDamage = 20;

        [TitleGroup("Insolence"), LabelText("Insolence Range"), MinValue(0)]
        public float insolenceRange = 10f;

        [TitleGroup("Insolence"), LabelText("Insolence Past Target Distance"), MinValue(0)]
        public float insolencePastTargetDistance = 10f;

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
        public int normalAttackDamage = 10;
        public int maxAttackCountInCombo;
        public float attackDuration;
        public float attackCancelTimer;
        public float attackSpawnHitBoxTimer;
        public float attackRemoveHitBoxTimer;
        public float attackDashDuration;
        public float attackDashDelay;
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
        [HideInInspector] public float tagCooldown;
        [HideInInspector] public float tagDuration;
        [HideInInspector] public float inactiveHealthRegenRate;
        [HideInInspector] public float inactiveStaminaRegenRate;
    }
}
