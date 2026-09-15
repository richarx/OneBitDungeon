using System;
using Player.Scripts;
using Tools_and_Scripts;
using UnityEngine;

namespace Interactable
{
    public class InputIconDisplay : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Space]
        [SerializeField] private Sprite gamepadIcon;
        [SerializeField] private Sprite keyboardIcon;

        [Space]
        [SerializeField] private Vector3 offsetPosition;
        [SerializeField] private float frequency;
        [SerializeField] private float amplitude;

        private PlayerStateMachine player;
        private DetectPlayerInRange detection;
        private InteractableItem interactableItem;

        private bool isDisplayed = false;

        private void Start()
        {
            player = PlayerStateMachine.instance;

            detection = transform.parent.GetComponent<DetectPlayerInRange>();
            interactableItem = transform.parent.GetComponent<InteractableItem>();

            detection.OnPlayerEnterRange.AddListener(TryDisplayIcon);
            detection.OnPlayerExitRange.AddListener(TryHideIcon);

            SetPosition();
            spriteRenderer.gameObject.SetActive(false);
        }

        private void Update()
        {
            TryDisplayIcon();

            if (!isDisplayed)
                return;

            TryHideIcon();

            SetSpriteFromInputType();
            AnimatePosition();
        }

        private void AnimatePosition()
        {
            Vector3 position = offsetPosition + Vector3.up * (Mathf.Sin(Time.time * frequency) * amplitude);
            transform.localPosition = position;
        }

        private void TryDisplayIcon()
        {
            if (!isDisplayed && detection.IsPlayerInRange && !player.isLocked && !interactableItem.isBeingUsed)
            {
                Debug.Log($"Zuzu : Show Icon : {interactableItem.isBeingUsed}");
                DisplayIcon();
                return;
            }
        }

        private void DisplayIcon()
        {
            if (isDisplayed)
                return;

            isDisplayed = true;

            SetPosition();
            SetSpriteFromInputType();
            StopAllCoroutines();
            StartCoroutine(Tools.Fade(spriteRenderer, 0.3f, true));
        }

        private void TryHideIcon()
        {
            if (isDisplayed && (!detection.IsPlayerInRange || player.isLocked || interactableItem.isBeingUsed))
            {
                Debug.Log($"Zuzu : Hide Icon : {isDisplayed}");
                HideIcon();
                return;
            }
        }


        private void HideIcon()
        {
            if (!isDisplayed)
                return;

            isDisplayed = false;

            StopAllCoroutines();
            StartCoroutine(Tools.Fade(spriteRenderer, 0.3f, false));
        }

        private void SetSpriteFromInputType()
        {
            bool isGamepad = player.inputPackage.lastInputType == InputType.Gamepad;
            spriteRenderer.sprite = isGamepad ? gamepadIcon : keyboardIcon;
        }

        private void SetPosition()
        {
            transform.localPosition = offsetPosition;
        }
    }
}
