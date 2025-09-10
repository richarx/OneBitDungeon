using System;
using UnityEngine;

namespace Player
{
    public class PlayerAnimation : MonoBehaviour
    {
        [SerializeField] private Animator animator;

        private PlayerStateMachine player;

        private void Start()
        {
            player = PlayerStateMachine.instance;
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
                case BehaviourType.Locked:
                    PlayIdleAnimation();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void PlayIdleAnimation()
        {
            animator.Play($"Idle_NoWeapon_{ComputeLookDirection()}");
        }

        private void PlayRunAnimation()
        {
            animator.Play($"Walk_NoWeapon_{ComputeLookDirection()}");
        }
        
        private void PlayRollAnimation()
        {
            animator.Play($"Roll_NoWeapon_{ComputeCardinalLookDirection()}");
        }

        private string ComputeLookDirection()
        {
            float angle = player.lastLookDirection.ToDegree();

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
            float angle = player.lastLookDirection.ToDegree();

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
    }
}
