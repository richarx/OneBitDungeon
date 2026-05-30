using System;
using Game_Manager;
using Tools_and_Scripts;
using UnityEngine;

namespace Player.Scripts
{
    public class PlayerAnimation : MonoBehaviour
    {
        [SerializeField] private Animator animator;

        private PlayerStateMachine player;

        public void SetAnimator(Animator newAnimator)
        {
            animator = newAnimator;
        }

        private void Start()
        {
            player = PlayerStateMachine.instance;

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
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void PlayDeathAnimation()
        {
            animator.Play($"Death_NoWeapon_{ComputeLeftRightLookDirection()}");
        }

        private void PlaySitAnimation()
        {
            string animationName = player.playerSit.IsGettingUp ? "GetUp" : "SitDown";
            animator.Play($"{animationName}_InBack_{ComputeLeftRightLookDirection()}");
        }

        private void PlayStartParryAnimation()
        {
            animator.Play($"ParryStart_InHand_{ComputeLookDirection()}", 0, player.playerParry.isFromParry ? 0.5f : 0.0f);
        }

        private void PlayRecoveryParryAnimation()
        {
            animator.Play($"ParryRecovery_InHand_{ComputeLookDirection()}");
        }

        private void PlaySuccessParryAnimation()
        {
            animator.Play($"Parry_Success_InHand_{ComputeLookDirection()}");
        }

        private void PlayIdleAnimation()
        {
            animator.Play($"Idle_{ComputeWeaponState()}_{ComputeLookDirection()}");
        }

        private void PlayRunAnimation()
        {
            animator.Play($"Walk_{ComputeWeaponState()}_{ComputeLookDirection()}");
        }

        private void PlayRollAnimation()
        {
            animator.Play($"Roll_NoWeapon_{ComputeCardinalLookDirection()}");
        }

        private void PlayJumpAnimation()
        {
            animator.Play($"Jump_{ComputeWeaponState()}_{ComputeLookDirection()}");
        }

        private void PlayAttackAnimation(string attackAnimation)
        {
            animator.Play($"{attackAnimation}_InHand_{ComputeLookDirection()}", 0, 0.0f);
        }

        private void PlayStaggerAnimation()
        {
            animator.Play($"Hurt_{ComputeWeaponState()}_{ComputeLookDirection()}");
        }

        private string ComputeWeaponState()
        {
            if (!player.playerSword.CurrentlyHasSword)
                return "NoWeapon";

            if (player.playerSword.IsSwordInHand)
                return "InHand";

            return "InBack";
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

        private string ComputeCardinalLookDirection()
        {
            float angle = player.LastLookDirection.ToSignedDegree();

            if (angle < 0)
                angle = 360.0f + angle;

            if (angle > 45.0f && angle <= 135.0f)
                return "B";

            if (angle >= 135.0f && angle <= 225.0f)
                return "L";

            if (angle > 225.0f && angle <= 315.0f)
                return "F";

            if ((angle > 315.0f && angle <= 360.0f) || (angle >= 0.0f && angle <= 45.0f))
                return "R";

            return "F";
        }

        private string ComputeLeftRightLookDirection()
        {
            return player.LastLookDirection.x <= 0.0f ? "L" : "R";
        }
    }
}
