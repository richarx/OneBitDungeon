using Player.Scripts;
using UnityEngine;

namespace Enemies.Goon
{
    public class GoonStateMachine : MonoBehaviour
    {
        public GoonData goonData;
        
        public IGoonBehaviour currentBehaviour;
        
        // Behaviour States
        public GoonIdle goonIdle = new GoonIdle();
        public GoonWalk goonWalk = new GoonWalk();
        public GoonStagger goonStagger = new GoonStagger();
        public GoonDead goonDead = new GoonDead();
        
        
        [HideInInspector] public Rigidbody rb;
        [HideInInspector] public Damageable damageable;
        
        [HideInInspector] public Vector3 moveVelocity;
        [HideInInspector] public Vector2 lastLookDirection = Vector2.right;
        
        public Vector3 position => transform.position;

        
        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            damageable = GetComponent<Damageable>();
            
            damageable.OnTakeDamage.AddListener(() => ChangeBehaviour(goonStagger));
            
            lastLookDirection = Vector2.right;

            currentBehaviour = goonWalk;
            currentBehaviour.StartBehaviour(this, BehaviourType.Idle);
        }

        private void Update()
        {
            currentBehaviour.UpdateBehaviour(this);
        }
        
        private void FixedUpdate()
        {
            currentBehaviour.FixedUpdateBehaviour(this);
        }
        
        public void ChangeBehaviour(IGoonBehaviour newBehaviour)
        {
            if (newBehaviour == null || newBehaviour == currentBehaviour)
                return;

            BehaviourType previous = currentBehaviour.GetBehaviourType();
            currentBehaviour.StopBehaviour(this, newBehaviour.GetBehaviourType());
            currentBehaviour = newBehaviour;
            
            currentBehaviour.StartBehaviour(this, previous);
        }
        
        public void ApplyMovement()
        {
            rb.velocity = moveVelocity;
        }

        public Vector3 ComputeDirectionToPlayer()
        {
            return (PlayerStateMachine.instance.position - position).normalized;
        }
    }
}
