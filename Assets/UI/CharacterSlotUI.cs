using Player.Scripts;
using UnityEngine;

namespace UI
{
    public class CharacterSlotUI : MonoBehaviour
    {
        [SerializeField] private int slotIndex;
        [SerializeField] private CanvasGroup canvasGroup;

        [SerializeField] private float activeAlpha = 1.0f;
        [SerializeField] private float inactiveAlpha = 0.2f;

        private void OnEnable()
        {
            PlayerTagSystem.OnTagSwap.AddListener(OnTagSwap);
        }

        private void OnDisable()
        {
            PlayerTagSystem.OnTagSwap.RemoveListener(OnTagSwap);
        }

        private void OnTagSwap(int activeSlotIndex)
        {
            canvasGroup.alpha = slotIndex == activeSlotIndex ? activeAlpha : inactiveAlpha;
        }
    }
}
