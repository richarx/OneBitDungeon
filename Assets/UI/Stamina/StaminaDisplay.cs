using Player.Scripts;
using Tools_and_Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Stamina
{
    public class StaminaDisplay : MonoBehaviour
    {
        [SerializeField] private Image staminaBar;
        [SerializeField] private Image leftCorner;
        [SerializeField] private Image rightCorner;
        [SerializeField] private float smoothTime;
        [SerializeField] private Color filledColor;
        [SerializeField] private Color emptyColor;
        [SerializeField] private GameObject pivot;
        [SerializeField] private int characterIndex = 0;

        private PlayerStateMachine player;
        private PlayerStamina playerStamina;
        private PlayerData playerData;

        private float velocity;

        private bool isDisplayed = true;
        
        private void Awake()
        {
            PlayerLocked.OnLockPlayer.AddListener(() =>
            {
                if (isDisplayed && PlayerStateMachine.instance.playerLocked.GetLockState == PlayerLocked.LockState.Hidden)
                    HideInstant();
            });
            PlayerLocked.OnUnlockPlayer.AddListener(() =>
            {
                if (!isDisplayed)
                    DisplayStaminaBar();
            });
        }

        private void Start()
        {
            player = PlayerStateMachine.instance;
            playerStamina = player.playerStamina;
            playerData = player.playerData;
        }

        private void Update()
        {
            float current = staminaBar.fillAmount;
            float target = Tools.NormalizeValue(GetCurrentStamina(), 0.0f, GetMaxStamina());

            staminaBar.fillAmount = Mathf.SmoothDamp(current, target, ref velocity, smoothTime);

            bool isEmpty = GetCurrentStamina() <= 0f;
            Color color = isEmpty ? emptyColor : filledColor;
            leftCorner.color = color;
            rightCorner.color = color;
        }
        
        private void DisplayStaminaBar()
        {
            pivot.SetActive(true);
            isDisplayed = true;
        }

        private void HideInstant()
        {
            pivot.SetActive(false);
            isDisplayed = false;
        }

        private float GetCurrentStamina()
        {
            var tagSystem = player.playerTagSystem;
            if (tagSystem == null) return playerStamina.CurrentStamina;
            if (characterIndex == tagSystem.ActiveSlotIndex) return playerStamina.CurrentStamina;
            return tagSystem.GetSlot(characterIndex).savedStamina;
        }

        private float GetMaxStamina()
        {
            var tagSystem = player.playerTagSystem;
            if (tagSystem == null) return playerData.maxStamina;
            var def = tagSystem.GetSlot(characterIndex).definition;
            return def != null && def.playerData != null ? def.playerData.maxStamina : playerData.maxStamina;
        }
    }
}
