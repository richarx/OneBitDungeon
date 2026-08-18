using System;
using Player.Scripts;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Arrogance
{
    public class ArroganceDisplay : MonoBehaviour
    {
        [Required, SerializeField] private Image arroganceBar;
        [SerializeField] private float smoothTime = 0.1f;
        [SerializeField] private TextMeshProUGUI textMeshPro;

        private float velocity;

        private bool isFilled;

        private void Update()
        {
            if (PlayerStateMachine.instance == null || PlayerStateMachine.instance.playerArrogance == null)
                return;

            float current = arroganceBar.fillAmount;
            float target = PlayerStateMachine.instance.playerArrogance.NormalizedArrogance;

            arroganceBar.fillAmount = Mathf.SmoothDamp(current, target, ref velocity, smoothTime);

            bool isCurrentlyFilled = target >= 1.0f;

            if (isFilled != isCurrentlyFilled)
            {
                isFilled = isCurrentlyFilled;
                UpdateText();
            }
        }

        private void UpdateText()
        {
            if (isFilled)
                textMeshPro.text = "<bounce a*3 s*2>Insolence</bounce>";
            else
                textMeshPro.text = "Arrogance";
        }
    }
}
