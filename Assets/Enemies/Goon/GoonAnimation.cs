using System;
using Tools_and_Scripts;
using UnityEngine;

namespace Enemies.Goon
{
    public class GoonAnimation : MonoBehaviour
    {
        [SerializeField] private Animator animator;

        private GoonStateMachine goon;

        private void Start()
        {
            goon = GetComponent<GoonStateMachine>();
            goon.goonStagger.OnGetStaggered.AddListener(PlayStaggerAnimation);
            goon.goonSwordAttack.OnGoonSwordAttack.AddListener(PlayAttackAnimation);
            goon.goonDash.OnGoonDash.AddListener(PlayDashAnimation);
        }

        private void LateUpdate()
        {
            if (goon == null)
                Debug.Log("Goon is null");

            if (goon.currentBehaviour == null)
                Debug.Log("Goon current behaviour is null");

            
            switch (goon.currentBehaviour.GetBehaviourType())
            {
                case BehaviourType.Idle:
                    PlayIdleAnimation();
                    break;
                case BehaviourType.Walk:
                case BehaviourType.Approach:
                case BehaviourType.Strafe:
                    PlayRunAnimation();
                    break;
                case BehaviourType.Stagger:
                    break;
                case BehaviourType.Dead:
                    PlayDeadAnimation();
                    break;
                case BehaviourType.Attack:
                    if (goon.goonSwordAttack.IsAnticipationPhase)
                        PlayAttackAnticipation();
                    break;
                case BehaviourType.Dash:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void PlayIdleAnimation()
        {
            animator.Play($"Idle_Sword_{ComputeLookDirection()}");
        }

        private void PlayRunAnimation()
        {
            animator.Play($"Walk_Sword_{ComputeLookDirection()}");
        }
        
        private void PlayStaggerAnimation()
        {
            animator.Play($"Hurt_Sword_{ComputeLookDirection()}", 0, 0.0f);
        }
        
        private void PlayDeadAnimation()
        {
            animator.Play($"Die_Sword_{ComputeLookDirection()}");
        }

        private void PlayAttackAnticipation()
        {
            animator.Play($"Attack_Anticipation_Sword_{ComputeLookDirection()}");
        }
        
        private void PlayAttackAnimation(string attackAnimation)
        {
            animator.Play($"Attack_{attackAnimation}_{ComputeLookDirection()}", 0, 0.0f);
        }
        
        private void PlayDashAnimation()
        {
            animator.Play($"Dash_Sword_{ComputeLookDirection()}");
        }
        
        private string ComputeLookDirection()
        {
            float angle = goon.LastLookDirection.ToSignedDegree();

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
    }
}
