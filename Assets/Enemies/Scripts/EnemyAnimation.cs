using System;
using Enemies.Scripts.Behaviours;
using Tools_and_Scripts;
using UnityEngine;

namespace Enemies.Scripts
{
    public class EnemyAnimation : MonoBehaviour
    {
        [SerializeField] private Animator animator;

        private EnemyStateMachine enemy;

        private void Start()
        {
            enemy = GetComponent<EnemyStateMachine>();
            enemy.enemyStagger.OnGetStaggered.AddListener(PlayStaggerAnimation);
            enemy.OnAttack.AddListener((animationName, direction) => PlayAttackAnimation(animationName));
        }

        private void LateUpdate()
        {
            if (enemy == null)
                Debug.Log("Goon is null");

            if (enemy.currentBehaviour == null)
                Debug.Log("Goon current behaviour is null");

            
            switch (enemy.currentBehaviour.GetBehaviourType())
            {
                case BehaviourType.Spawn:
                    PlaySpawnAnimation();
                    break;
                case BehaviourType.Idle:
                    PlayIdleAnimation();
                    break;
                case BehaviourType.Walk:
                    PlayRunAnimation();
                    break;
                case BehaviourType.Dead:
                    PlayDeadAnimation();
                    break;
                case BehaviourType.Stagger:
                case BehaviourType.Attack:
                case BehaviourType.Dash:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void PlaySpawnAnimation()
        {
            if (enemy.enemySpawn.isLocked)
                PlayIdleAnimation();
            else
                PlayRunAnimation();
        }

        private void PlayIdleAnimation()
        {
            animator.Play($"Idle_{ComputeLookDirection()}");
        }

        private void PlayRunAnimation()
        {
            animator.Play($"Walk_{ComputeLookDirection()}");
        }
        
        private void PlayStaggerAnimation()
        {
            animator.Play($"Hurt_{ComputeLookDirection()}", 0, 0.0f);
        }
        
        private void PlayDeadAnimation()
        {
            animator.Play($"Die_{ComputeLookDirection()}");
        }

        private void PlayAttackAnimation(string attackAnimation)
        {
            animator.Play($"{attackAnimation}_{ComputeLookDirection()}", 0, 0.0f);
        }
        
        public void PlayAnimation(string animationName)
        {
            animator.Play($"{animationName}_{ComputeLookDirection()}");
        }
        
        private string ComputeLookDirection()
        {
            float angle = enemy.LastLookDirection.ToSignedDegree();

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
