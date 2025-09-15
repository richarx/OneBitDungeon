using System;
using UnityEngine;

namespace Player.Scripts
{
    public class PlayerStamina : MonoBehaviour
    {
        private PlayerData playerData;

        private float currentStamina;
        public float CurrentStamina => currentStamina;
        public bool IsEmpty => currentStamina <= 0.0f;
        public bool IsFull => currentStamina >= playerData.maxStamina;

        private float lastStaminaUseTimestamp;
        
        private void Start()
        {
            playerData = PlayerStateMachine.instance.playerData;
            currentStamina = playerData.maxStamina;
        }

        private void Update()
        {
            if (!IsEmpty && !IsFull && Time.time - lastStaminaUseTimestamp >= playerData.staminaCooldown)
                RefillStamina();
            
            if (IsEmpty && Time.time - lastStaminaUseTimestamp >= playerData.staminaEmptyCooldown)
                RefillStamina();
        }

        private void RefillStamina()
        {
            currentStamina = Mathf.Min(playerData.maxStamina, currentStamina + playerData.refillRate * Time.deltaTime);
        }

        public void ConsumeStamina(float amount)
        {
            currentStamina = Mathf.Max(0.0f, currentStamina - amount);
            lastStaminaUseTimestamp = Time.time;
        }
    }
}
