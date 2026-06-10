using System;
using Game_Manager;
using Interactable;
using Tools_and_Scripts;
using UnityEngine;

namespace Player.Scripts
{
    public class PlayerStateMachine : MonoBehaviour
    {
        public PlayerData playerData;
        public LayerMask obstaclesLayer;
        public GameObject graphics;

        public static PlayerStateMachine instance;

        // Behaviour States
        public PlayerIdle playerIdle = new PlayerIdle();
        public PlayerRun playerRun = new PlayerRun();
        public PlayerRoll playerRoll = new PlayerRoll();
        public PlayerJump playerJump = new PlayerJump();
        public PlayerStagger playerStagger = new PlayerStagger();
        public PlayerParry playerParry = new PlayerParry();
        public PlayerSit playerSit = new PlayerSit();
        public PlayerDead playerDead = new PlayerDead();
        public PlayerAttack playerAttack;
        public PlayerTag playerTag = new PlayerTag();
        public PlayerLocked playerLocked = new PlayerLocked();

        public IPlayerBehaviour currentBehaviour;

        public Vector3 position => transform.position;
        public float hitBoxRadius => 0.15f;
        public bool isLocked => currentBehaviour.GetBehaviourType() == BehaviourType.Locked || playerRun.IsSkippingFrame;
        public bool isLockedAndHidden => currentBehaviour.GetBehaviourType() == BehaviourType.Locked && playerLocked.GetLockState == PlayerLocked.LockState.Hidden;

        [HideInInspector] public Vector2 moveInput;
        [HideInInspector] public Vector3 moveVelocity;

        [HideInInspector] public Rigidbody rb;
        [HideInInspector] public PlayerSword playerSword;
        [HideInInspector] public PlayerTargeting playerTargeting;
        [HideInInspector] public PlayerHealth playerHealth;
        [HideInInspector] public PlayerStamina playerStamina;
        [HideInInspector] public PlayerInteraction playerInteraction;
        [HideInInspector] public PlayerTagSystem playerTagSystem;
        [HideInInspector] public PlayerAnimation playerAnimation;
        [HideInInspector] public CodeAnimator codeAnimator;

        [HideInInspector] public InputPacker inputPacker = new InputPacker();
        [HideInInspector] public InputPackage inputPackage = new InputPackage();

        private Vector2 lastLookDirection = Vector2.right;
        public Vector2 LastLookDirection => lastLookDirection;

        private void Awake()
        {
            instance = this;
            rb = GetComponent<Rigidbody>();
            playerSword = GetComponent<PlayerSword>();
            playerTargeting = GetComponent<PlayerTargeting>();
            playerHealth = GetComponent<PlayerHealth>();
            playerStamina = GetComponent<PlayerStamina>();
            playerInteraction = GetComponent<PlayerInteraction>();
            playerAnimation = GetComponent<PlayerAnimation>();
            codeAnimator = GetComponent<CodeAnimator>();
            playerAttack = new PlayerAttack(this, new Character1AttackStrategy());

            lastLookDirection = Vector2.right;
            currentBehaviour = playerIdle;
            currentBehaviour.StartBehaviour(this, BehaviourType.Idle);
        }

        private void Start()
        {
            if (!Application.isEditor)
                Cursor.visible = false;

            playerHealth.OnPlayerTakeDamage.AddListener((direction) => playerStagger.TriggerStagger(this, direction));
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

        public void TeleportPlayer(Vector3 newPosition)
        {
            newPosition.y = 0.0f;
            rb.MovePosition(newPosition);
        }

        private void KeepOnGround()
        {
            Vector3 rbPosition = rb.position;
            rb.MovePosition(rbPosition + Vector3.up * -rbPosition.y);
        }

        public void ComputeLastLookDirection()
        {
            if (playerTargeting.hasTarget && playerSword.IsSwordInHand)
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
            KeepOnGround();
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

        public bool CheckForInteraction()
        {
            if (inputPackage.GetInteraction.wasPressedThisFrame)
            {
                if (playerInteraction.IsInteractableInRange)
                    playerInteraction.InteractWithItem();
                else
                    playerSword.SwapSword();

                return true;
            }

            return false;
        }

        public bool IsAllowedToInteract()
        {
            BehaviourType current = currentBehaviour.GetBehaviourType();

            return current != BehaviourType.Sit;
        }

        public int ComputeCurrentDamage()
        {
            return 10;
        }
    }
}
