using System;
using Player.Sword_Hitboxes;
using UnityEngine;
using UnityEngine.Events;

namespace Player.Scripts
{
    public class PlayerTagSystem : MonoBehaviour
    {
        [Serializable]
        public class CharacterSlot
        {
            public CharacterDefinition definition;
            public GameObject graphicsObject;

            [HideInInspector] public int savedHealth;
            [HideInInspector] public float savedStamina;
        }

        [SerializeField] private CharacterSlot[] slots = new CharacterSlot[2];

        private int activeSlotIndex;
        private float lastSwapTimestamp = float.MinValue;
        private float inactiveHealthAccumulator;

        private PlayerStateMachine player;

        public int ActiveSlotIndex => activeSlotIndex;
        public bool CanTag => Time.time - lastSwapTimestamp >= player.playerData.tagCooldown;
        public float TagCooldownProgress => Mathf.Clamp01((Time.time - lastSwapTimestamp) / player.playerData.tagCooldown);
        public CharacterSlot GetSlot(int index) => slots[index];

        public static UnityEvent<int> OnTagSwap = new UnityEvent<int>();

        private void Start()
        {
            player = PlayerStateMachine.instance;
            player.playerTagSystem = this;

            activeSlotIndex = 0;

            for (int i = 0; i < slots.Length; i++)
            {
                slots[i].savedHealth = slots[i].definition != null ? slots[i].definition.maxHealth : 0;
                slots[i].savedStamina = slots[i].definition?.playerData != null ? slots[i].definition.playerData.maxStamina : 0f;
            }

            if (slots[0].graphicsObject != null) slots[0].graphicsObject.SetActive(true);
            if (slots[1].graphicsObject != null) slots[1].graphicsObject.SetActive(false);


            OnTagSwap?.Invoke(activeSlotIndex);
        }

        private void Update()
        {
            if (slots[0].definition == null || slots[1].definition == null) return;

            int inactiveIndex = 1 - activeSlotIndex;
            CharacterSlot inactiveSlot = slots[inactiveIndex];
            PlayerData inactiveData = inactiveSlot.definition.playerData;

            // Stamina regen
            inactiveSlot.savedStamina = Mathf.Min(
                inactiveSlot.savedStamina + inactiveData.inactiveStaminaRegenRate * Time.deltaTime,
                inactiveData.maxStamina);

            // Health regen (int-based accumulator)
            inactiveHealthAccumulator += inactiveData.inactiveHealthRegenRate * Time.deltaTime;
            if (inactiveHealthAccumulator >= 1f)
            {
                int toAdd = (int)inactiveHealthAccumulator;
                inactiveSlot.savedHealth = Mathf.Clamp(inactiveSlot.savedHealth + toAdd, 0, inactiveSlot.definition.maxHealth);
                inactiveHealthAccumulator -= toAdd;
            }
        }

        public void Swap()
        {
            if (!CanTag) return;

            int oldIndex = activeSlotIndex;
            int newIndex = 1 - activeSlotIndex;

            // Save active character state
            slots[oldIndex].savedHealth = player.playerHealth.CurrentHealth;
            slots[oldIndex].savedStamina = player.playerStamina.CurrentStamina;

            activeSlotIndex = newIndex;
            CharacterSlot newSlot = slots[newIndex];

            // Swap graphics
            if (slots[oldIndex].graphicsObject != null) slots[oldIndex].graphicsObject.SetActive(false);
            if (newSlot.graphicsObject != null) newSlot.graphicsObject.SetActive(true);

            // Update player references
            player.playerData = newSlot.definition.playerData;
            player.graphics = newSlot.graphicsObject;

            // Update attack strategy
            player.playerAttack.SetStrategy(newSlot.definition.attackStrategy);

            // Update animator
            if (newSlot.graphicsObject != null)
            {
                SpriteRenderer newRenderer = newSlot.graphicsObject.GetComponentInChildren<SpriteRenderer>();
                if (newRenderer != null)
                {
                    player.codeAnimator.SetGraphicsTarget(newRenderer);
                    player.codeAnimator.SetAnimationsHolder(newSlot.definition.animationsHolder);
                }
            }

            // Update stamina's cached playerData reference
            player.playerStamina.SetPlayerData(newSlot.definition.playerData);

            // Update health max and restore saved state
            player.playerHealth.SetMaxHealth(newSlot.definition.maxHealth);
            player.playerHealth.SetHealth(newSlot.savedHealth);
            player.playerStamina.SetStamina(newSlot.savedStamina);

            lastSwapTimestamp = Time.time;
            inactiveHealthAccumulator = 0f;
            OnTagSwap?.Invoke(activeSlotIndex);
        }

        public void ResetBothCharacters()
        {
            // Reset inactive slot
            int inactiveIndex = 1 - activeSlotIndex;
            slots[inactiveIndex].savedHealth = slots[inactiveIndex].definition.maxHealth;
            slots[inactiveIndex].savedStamina = slots[inactiveIndex].definition.playerData.maxStamina;

            // Reset active character via components
            player.playerHealth.SetMaxHealth(slots[activeSlotIndex].definition.maxHealth);
            player.playerHealth.SetHealth(slots[activeSlotIndex].definition.maxHealth);
            player.playerStamina.SetStamina(player.playerData.maxStamina);
        }
    }
}
