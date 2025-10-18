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

        private bool isDisplayed = false;
        
        private void Start()
        {
            player = PlayerStateMachine.instance;

            detection = transform.parent.GetComponent<DetectPlayerInRange>();
            
            detection.OnPlayerEnterRange.AddListener(DisplayIcon);
            detection.OnPlayerExitRange.AddListener(HideIcon);
            
            SetPosition();
            spriteRenderer.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (!isDisplayed)
                return;
            
            if (isDisplayed && !detection.IsPlayerInRange)
            {
                HideIcon();
                return;
            }
            
            SetSpriteFromInputType();
            AnimatePosition();
        }

        private void AnimatePosition()
        {
            Vector3 position = offsetPosition + Vector3.up * (Mathf.Sin(Time.time * frequency) * amplitude);
            transform.localPosition = position;
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
