using System;
using Level_Holder;
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
        
        private int currentHealth;

        public int CurrentHealth => currentHealth;
        public int StartingHealth => startingHealth;
        public bool IsDead => currentHealth <= 0;
        public bool IsFullLife => currentHealth == startingHealth;

        private void Awake()
        {
            ResetHealth();
        }

        private void Start()
        {
            LevelHolder.OnRestartGame.AddListener(ResetHealth);
        }

        private void Update()
        {
            if (Gamepad.current.leftShoulder.wasPressedThisFrame)
                TakeDamage(1, Vector3.right);

            if (Gamepad.current.rightShoulder.wasPressedThisFrame)
                ResetHealth();
        }

        private void ResetHealth()
        {
            currentHealth = startingHealth;
        }

        public void TakeDamage(int damage, Vector3 direction)
        {
            if (IsDead)
                return;

            currentHealth -= damage;
            
            if (IsDead)
                OnPlayerDie?.Invoke();
            else
                OnPlayerTakeDamage?.Invoke(direction);
        }
    }
}
