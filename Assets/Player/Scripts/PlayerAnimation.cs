using System;
using Game_Manager;
using Tools_and_Scripts;
using UnityEngine;
using static CodeAnimator;

namespace Player.Scripts
{
    public class PlayerAnimation : MonoBehaviour
    {
        [SerializeField] private Animator animator;

        private PlayerStateMachine player;
        private CodeAnimator codeAnimator;

        public void SetAnimator(Animator newAnimator)
        {
            animator = newAnimator;
        }

        private void Start()
        {
            player = PlayerStateMachine.instance;
            codeAnimator = GetComponent<CodeAnimator>();

            player.playerAttack.OnPlayerAttack.AddListener(PlayAttackAnimation);
            player.playerStagger.OnStagger.AddListener(PlayStaggerAnimation);

            player.playerSit.OnStartSittingDown.AddListener(PlaySitAnimation);
            player.playerSit.OnStartGettingUp.AddListener(PlaySitAnimation);

            player.playerParry.OnStartParry.AddListener(PlayStartParryAnimation);
            player.playerParry.OnStopParry.AddListener(PlayRecoveryParryAnimation);
            player.playerParry.OnSuccessfulParry.AddListener(PlaySuccessParryAnimation);
        }

        private void LateUpdate()
        {
            switch (player.currentBehaviour.GetBehaviourType())
            {
                case BehaviourType.Idle:
                    PlayIdleAnimation();
                    break;
                case BehaviourType.Run:
                    PlayRunAnimation();
                    break;
                case BehaviourType.Roll:
                    PlayRollAnimation();
                    break;
                case BehaviourType.Jump:
                    PlayJumpAnimation();
                    break;
                case BehaviourType.Sit:
                    if (player.playerSit.isRotating)
                        PlayIdleAnimation();
                    break;
                case BehaviourType.Dead:
                    PlayDeathAnimation();
                    break;
                case BehaviourType.Parry:
                case BehaviourType.Attack:
                case BehaviourType.Stagger:
                    break;
                case BehaviourType.Locked:
                    if (!player.isLockedAndHidden)
                    {
                        if (player.moveVelocity.magnitude >= 0.015f)
                            PlayRunAnimation();
                        else
                            PlayIdleAnimation();
                    }
                    break;
                case BehaviourType.Tag:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void PlayDeathAnimation()
        {
            codeAnimator.PlayAnimation(AnimationType.Die, ComputeLeftRightAnimationDirection());
            //animator.Play($"Death_NoWeapon_{ComputeLeftRightLookDirection()}");
        }

        private void PlaySitAnimation()
        {
            if (player.playerSit.IsGettingUp)
                codeAnimator.PlayAnimation(AnimationType.GetUp, ComputeLeftRightAnimationDirection());
            else
                codeAnimator.PlayAnimation(AnimationType.SitDown, ComputeLeftRightAnimationDirection());

            //string animationName = player.playerSit.IsGettingUp ? "GetUp" : "SitDown";
            //animator.Play($"{animationName}_InBack_{ComputeLeftRightLookDirection()}");
        }

        private void PlayStartParryAnimation()
        {
            codeAnimator.PlayAnimation(AnimationType.ParryStart, ComputeAnimationDirection(), player.playerSword.IsSwordInHand);
            //animator.Play($"ParryStart_InHand_{ComputeLookDirection()}", 0, player.playerParry.isFromParry ? 0.5f : 0.0f);
        }

        private void PlayRecoveryParryAnimation()
        {
            codeAnimator.PlayAnimation(AnimationType.ParryRecovery, ComputeAnimationDirection(), player.playerSword.IsSwordInHand);
            //animator.Play($"ParryRecovery_InHand_{ComputeLookDirection()}");
        }

        private void PlaySuccessParryAnimation()
        {
            codeAnimator.PlayAnimation(AnimationType.ParrySuccess, ComputeAnimationDirection(), player.playerSword.IsSwordInHand);
            //animator.Play($"Parry_Success_InHand_{ComputeLookDirection()}");
        }

        private void PlayIdleAnimation()
        {
            codeAnimator.PlayAnimation(AnimationType.Idle, ComputeAnimationDirection(), player.playerSword.IsSwordInHand);
            //animator.Play($"Idle_{ComputeWeaponState()}_{ComputeLookDirection()}");
        }

        private void PlayRunAnimation()
        {
            codeAnimator.PlayAnimation(AnimationType.Walk, ComputeAnimationDirection(), player.playerSword.IsSwordInHand);
            //animator.Play($"Walk_{ComputeWeaponState()}_{ComputeLookDirection()}");
        }

        private void PlayRollAnimation()
        {
            codeAnimator.PlayAnimation(AnimationType.Roll, ComputeCardinalAnimationDirection());
            //animator.Play($"Roll_NoWeapon_{ComputeCardinalLookDirection()}");
        }

        private void PlayJumpAnimation()
        {
            codeAnimator.PlayAnimation(AnimationType.Jump, ComputeAnimationDirection(), player.playerSword.IsSwordInHand);
            //animator.Play($"Jump_{ComputeWeaponState()}_{ComputeLookDirection()}");
        }

        private void PlayAttackAnimation(string attackAnimation)
        {
            codeAnimator.PlayAnimation(AnimationType.Attack, ComputeAnimationDirection(), true);
            //animator.Play($"{attackAnimation}_InHand_{ComputeLookDirection()}", 0, 0.0f);
        }

        private void PlayStaggerAnimation()
        {
            codeAnimator.PlayAnimation(AnimationType.Hurt, ComputeAnimationDirection(), player.playerSword.IsSwordInHand);
            //animator.Play($"Hurt_{ComputeWeaponState()}_{ComputeLookDirection()}");
        }

        private string ComputeLookDirection()
        {
            float angle = player.LastLookDirection.ToSignedDegree();

            if (angle < 0)
                angle = 360.0f + angle;

            if (angle > 15.0f && angle <= 60.0f)
                return "BR";

            if (angle > 60.0f && angle <= 120.0f)
                return "B";

            if (angle > 120.0f && angle < 165.0f)
                return "BL";

            if (angle >= 165.0f && angle <= 240.0f)
                return "L";

            if (angle > 240.0f && angle <= 300.0f)
                return "F";

            if ((angle > 300.0f && angle <= 360.0f) || (angle >= 0.0f && angle <= 15.0f))
                return "R";

            return "F";
        }

        private AnimationDirection ComputeAnimationDirection()
        {
            float angle = player.LastLookDirection.ToSignedDegree();

            if (angle < 0)
                angle = 360.0f + angle;

            if (angle > 15.0f && angle <= 60.0f)
                return AnimationDirection.BR;

            if (angle > 60.0f && angle <= 120.0f)
                return AnimationDirection.B;

            if (angle > 120.0f && angle < 165.0f)
                return AnimationDirection.BL;

            if (angle >= 165.0f && angle <= 240.0f)
                return AnimationDirection.L;

            if (angle > 240.0f && angle <= 300.0f)
                return AnimationDirection.F;

            if ((angle > 300.0f && angle <= 360.0f) || (angle >= 0.0f && angle <= 15.0f))
                return AnimationDirection.R;

            return AnimationDirection.F;
        }

        private AnimationDirection ComputeCardinalAnimationDirection()
        {
            float angle = player.LastLookDirection.ToSignedDegree();

            if (angle < 0)
                angle = 360.0f + angle;

            if (angle > 45.0f && angle <= 135.0f)
                return AnimationDirection.B;

            if (angle >= 135.0f && angle <= 225.0f)
                return AnimationDirection.L;

            if (angle > 225.0f && angle <= 315.0f)
                return AnimationDirection.F;

            if ((angle > 315.0f && angle <= 360.0f) || (angle >= 0.0f && angle <= 45.0f))
                return AnimationDirection.R;

            return AnimationDirection.F;
        }

        private AnimationDirection ComputeLeftRightAnimationDirection()
        {
            return player.LastLookDirection.x <= 0.0f ? AnimationDirection.L : AnimationDirection.R;
        }
    }
}
