using Player.Scripts;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Arrogance
{
    public class ArroganceDisplay : MonoBehaviour
    {
        [Required, SerializeField] private Image arroganceBar;
        [SerializeField] private float smoothTime = 0.1f;

        private float velocity;

        private void Update()
        {
            if (PlayerStateMachine.instance == null || PlayerStateMachine.instance.playerArrogance == null)
                return;

            float current = arroganceBar.fillAmount;
            float target = PlayerStateMachine.instance.playerArrogance.NormalizedArrogance;

            arroganceBar.fillAmount = Mathf.SmoothDamp(current, target, ref velocity, smoothTime);
        }
    }
}
