using Level_Holder;
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
        public PlayerStagger playerStagger = new PlayerStagger();
        public PlayerParry playerParry = new PlayerParry();
        public PlayerAttack playerAttack;
        public PlayerLocked playerLocked = new PlayerLocked();

        public IPlayerBehaviour currentBehaviour;
        
        public Vector3 position => transform.position;
        public bool isLocked => currentBehaviour.GetBehaviourType() == BehaviourType.Locked || playerRun.IsSkippingFrame;

        [HideInInspector] public Vector2 moveInput;
        [HideInInspector] public Vector3 moveVelocity;

        [HideInInspector] public Rigidbody rb;
        [HideInInspector] public PlayerSword playerSword;
        [HideInInspector] public PlayerTargeting playerTargeting;
        [HideInInspector] public PlayerHealth playerHealth;

        [HideInInspector] public InputPacker inputPacker = new InputPacker();
        [HideInInspector] public InputPackage inputPackage;

        private Vector2 lastLookDirection = Vector2.right;
        public Vector2 LastLookDirection => lastLookDirection;

        private void Awake()
        {
            instance = this;
            rb = GetComponent<Rigidbody>();
            playerSword = GetComponent<PlayerSword>();
            playerTargeting = GetComponent<PlayerTargeting>();
            playerHealth = GetComponent<PlayerHealth>();
            playerAttack = new PlayerAttack(this);
        }

        private void Start()
        {
            if (!Application.isEditor)
                Cursor.visible = false;

            lastLookDirection = Vector2.right;

            playerHealth.OnPlayerTakeDamage.AddListener((direction) => playerStagger.TriggerStagger(this, direction));
            playerHealth.OnPlayerDie.AddListener(() => playerLocked.SetLockState(this));
            LevelHolder.OnResetGame.AddListener(() => transform.position = Vector3.zero);
            LevelHolder.OnRestartGame.AddListener(() => ChangeBehaviour(playerIdle));
            
            currentBehaviour = playerIdle;
            currentBehaviour.StartBehaviour(this, BehaviourType.Run);
        }
        
        private void Update()
        {
            inputPackage = inputPacker.ComputeInputPackage();
            moveInput = inputPackage.GetMove;

            //if (PauseMenu.instance.IsPaused)
            //    return;
            
            playerTargeting.ComputeTarget(this);

            currentBehaviour.UpdateBehaviour(this);
        }

        public void ComputeLastLookDirection()
        {
            if (playerTargeting.hasTarget)
                lastLookDirection = playerTargeting.directionToTarget.ToVector2().normalized;
            else if (moveInput.magnitude >= 0.15f)
                lastLookDirection = moveInput.normalized;
        }

        public void SetLastLookDirection(Vector2 direction)
        {
            lastLookDirection = direction.normalized;
        }

        private void FixedUpdate()
        {
            currentBehaviour.FixedUpdateBehaviour(this);
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
            moveVelocity.y = 0.0f;
        }
    }
}
