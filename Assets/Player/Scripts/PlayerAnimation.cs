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

        private void Start()
        {
            player = PlayerStateMachine.instance;
            player.playerAttack.OnPlayerAttack.AddListener(PlayAttackAnimation);
            player.playerStagger.OnStagger.AddListener(PlayStaggerAnimation);
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
                case BehaviourType.Parry:
                    PlayParryAnimation();
                    break;
                case BehaviourType.Sit:
                    PlaySitAnimation();
                    break;
                case BehaviourType.Dead:
                    PlayDeathAnimation();
                    break;
                case BehaviourType.Attack:
                case BehaviourType.Stagger:
                    break;
                case BehaviourType.Locked:
                    if (!player.isLockedAndHidden)
                        PlayIdleAnimation();
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
            string animationName = player.playerSit.IsGettingUp ? "GetUp" : "Sit";
            animator.Play($"{animationName}_InBack_L");
        }

        private void PlayParryAnimation()
        {
            string animationName = player.playerParry.WasSuccessful ? "Parry_Success" : "Parry";
            animator.Play($"{animationName}_InHand_{ComputeLookDirection()}");
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
