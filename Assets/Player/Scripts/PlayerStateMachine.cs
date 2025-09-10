using Tools_and_Scripts;
using UnityEngine;

namespace Player.Scripts
{
    public class PlayerStateMachine : MonoBehaviour
    {
        public PlayerData playerData;

        public static PlayerStateMachine instance;

        // Behaviour States
        public PlayerIdle playerIdle = new PlayerIdle();
        public PlayerRun playerRun = new PlayerRun();
        public PlayerRoll playerRoll = new PlayerRoll();
        public PlayerAttack playerAttack = new PlayerAttack();
        public PlayerLocked playerLocked = new PlayerLocked();

        public IPlayerBehaviour currentBehaviour;
        
        public Vector3 position => transform.position;
        public bool isLocked => currentBehaviour.GetBehaviourType() == BehaviourType.Locked || playerRun.IsSkippingFrame;

        [HideInInspector] public Vector2 moveInput;
        [HideInInspector] public Vector3 moveVelocity;

        [HideInInspector] public Rigidbody rb;
        [HideInInspector] public PlayerSword playerSword;

        [HideInInspector] public InputPacker inputPacker = new InputPacker();
        [HideInInspector] public InputPackage inputPackage;

        [HideInInspector] public Vector2 lastLookDirection = Vector2.right;
        
        private void Awake()
        {
            instance = this;
            rb = GetComponent<Rigidbody>();
            playerSword = GetComponent<PlayerSword>();
        }

        private void Start()
        {
            if (!Application.isEditor)
                Cursor.visible = false;

            lastLookDirection = Vector2.right;
            
            currentBehaviour = playerIdle;
            currentBehaviour.StartBehaviour(this, BehaviourType.Run);
        }
        
        private void Update()
        {
            inputPackage = inputPacker.ComputeInputPackage();
            
            //if (PauseMenu.instance.IsPaused)
            //    return;
            
            moveInput = inputPackage.GetMove;
            
            currentBehaviour.UpdateBehaviour(this);
        }

        private void FixedUpdate()
        {
            currentBehaviour.FixedUpdateBehaviour(this);
        }

        private void LateUpdate()
        {
            
        }

        public void ChangeBehaviour(IPlayerBehaviour newBehaviour)
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

        public bool IsMoving(float maxVelocity = 0.01f)
        {
            return ComputeGroundMoveVelocity().magnitude >= maxVelocity;
        }

        public Vector3 ComputeGroundMoveVelocity()
        {
            Vector3 velocity = moveVelocity;
            velocity.y = 0.0f;

            return velocity;
        }
    }
}
