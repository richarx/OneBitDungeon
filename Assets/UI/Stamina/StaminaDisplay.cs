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
            playerStamina = PlayerStateMachine.instance.playerStamina;
            playerData = PlayerStateMachine.instance.playerData;
        }

        private void Update()
        {
            float current = staminaBar.fillAmount;
            float target = Tools.NormalizeValue(playerStamina.CurrentStamina, 0.0f, playerData.maxStamina);

            staminaBar.fillAmount = Mathf.SmoothDamp(current, target, ref velocity, smoothTime);

            Color color = playerStamina.IsEmpty ? emptyColor : filledColor;
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
    }
}
