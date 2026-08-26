using Game_Manager;
using UnityEngine;
using UnityEngine.Events;

namespace Player.Scripts
{
    public class PlayerStamina : MonoBehaviour
    {
        public UnityEvent OnPlayerExhaustStamina = new UnityEvent();

        private PlayerStateMachine player;
        private PlayerData playerData;

        private float currentStamina;
        public float CurrentStamina => currentStamina;
        public bool IsEmpty => currentStamina <= 0.0f;
        public bool IsFull => currentStamina >= playerData.maxStamina;

        private float lastStaminaUseTimestamp;

        private void Start()
        {
            player = PlayerStateMachine.instance;

            playerData = player.playerData;
            currentStamina = playerData.maxStamina;
            player.playerHealth.OnPlayerTakeDamage.AddListener((_) =>
            {
                if (player.playerHealth.IsDead)
                    SetStamina(0.0f);
            });
            GameManager.OnRestartLevel.AddListener(() => SetStamina(playerData.maxStamina));
        }

        private void Update()
        {
            if (!player.playerHealth.IsDead && !IsEmpty && !IsFull && Time.time - lastStaminaUseTimestamp >= playerData.staminaCooldown)
                RefillStamina();

            if (!player.playerHealth.IsDead && IsEmpty && Time.time - lastStaminaUseTimestamp >= playerData.staminaEmptyCooldown)
                RefillStamina();
        }

        private void RefillStamina()
        {
            currentStamina = Mathf.Min(playerData.maxStamina, currentStamina + playerData.refillRate * Time.deltaTime);
        }

        public void GainStamina(float amount)
        {
            currentStamina = Mathf.Min(playerData.maxStamina, currentStamina + amount);
        }

        public void ConsumeStamina(float amount)
        {
            bool wasEmpty = IsEmpty;

            currentStamina = Mathf.Max(0.0f, currentStamina - amount);
            lastStaminaUseTimestamp = Time.time;

            if (!wasEmpty && IsEmpty)
                OnPlayerExhaustStamina?.Invoke();
        }

        public void SetStamina(float value)
        {
            currentStamina = Mathf.Clamp(value, 0.0f, playerData.maxStamina);
        }

        public void SetPlayerData(PlayerData data)
        {
            playerData = data;
        }
    }
}
