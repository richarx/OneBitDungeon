using System;
using Game_Manager;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Player.Scripts
{
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private int startingHealth;

        [HideInInspector] public UnityEvent<Vector3> OnPlayerTakeDamage = new UnityEvent<Vector3>();
        [HideInInspector] public UnityEvent OnPlayerDie = new UnityEvent();

        private PlayerStateMachine player;
        
        private int currentHealth;
        private float lastHitTimestamp;

        public int CurrentHealth => currentHealth;
        public int StartingHealth => startingHealth;
        public bool IsDead => currentHealth <= 0;
        public bool IsFullLife => currentHealth == startingHealth;
        public bool IsInvincibleFromLastHit => Time.time - lastHitTimestamp <= player.playerData.invincibilityDuration;

        
        private void Awake()
        {
            ResetHealth();
        }

        private void Start()
        {
            player = PlayerStateMachine.instance;
            GameManager.OnRestartLevel.AddListener(ResetHealth);
        }

        private void Update()
        {
            if (Application.isEditor)
            {
                if (Gamepad.current.dpad.left.wasPressedThisFrame)
                {
                    if (IsParrying())
                        TriggerParry();
                    else
                        TakeDamage(1, Vector3.right);
                }

                if (Gamepad.current.dpad.right.wasPressedThisFrame)
                    ResetHealth();    
            }
        }

        private void ResetHealth()
        {
            currentHealth = startingHealth;
        }

        public bool IsParrying()
        {
            return player.currentBehaviour.GetBehaviourType() == BehaviourType.Parry;
        }

        public void TriggerParry()
        {
            player.playerParry.TriggerSuccessfulParry();
        }

        public bool TakeDamage(int damage, Vector3 direction)
        {
            if (IsDead)
                return false;

            if (IsInvincibleFromLastHit)
                return false;

            currentHealth -= damage;
            lastHitTimestamp = Time.time;
            
            if (IsDead)
                OnPlayerDie?.Invoke();
            else
                OnPlayerTakeDamage?.Invoke(direction);

            return true;
        }
    }
}
