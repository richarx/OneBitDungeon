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
        
        
        public Vector3 position => transform.position;
        
        [HideInInspector] public Vector3 moveVelocity;

        [HideInInspector] public Rigidbody rb;
        
        [HideInInspector] public Vector2 lastLookDirection = Vector2.right;

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            
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
    }
}
