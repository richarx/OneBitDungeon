using System;
using Player.Scripts;
using Tools_and_Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Stamina
{
    public class StaminaDisplay : MonoBehaviour
    {
        [SerializeField] private Image staminaBar;
        [SerializeField] private float smoothTime;

        private PlayerStamina playerStamina;
        private PlayerData playerData;

        private float velocity;

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
        }
    }
}
