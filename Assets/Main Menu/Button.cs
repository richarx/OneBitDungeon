using SFX;
using TMPro;
using Tools_and_Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace Main_Menu
{
    public class Button : MonoBehaviour
    {
        [SerializeField] private Image border;
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private float clickAnimationDuration;
        [SerializeField] private AudioClip clickSound;

        private bool isDisplayed = true;

        private Vector3 startingScale;
        
        private void Start()
        {
            startingScale = border.rectTransform.localScale;
        }

        public void Display()
        {
            if (isDisplayed)
                return;

            border.gameObject.SetActive(true);
            text.gameObject.SetActive(true);
            
            StopAllCoroutines();
            StartCoroutine(Tools.Fade(border, 3.0f, true));
            StartCoroutine(Tools.Fade(text, 3.0f, true));

            isDisplayed = true;
        }
        
        public void Hide()
        {
            if (!isDisplayed)
                return;
            
            StopAllCoroutines();
            StartCoroutine(Tools.Fade(border, 1.0f, false));
            StartCoroutine(Tools.Fade(text, 1.0f, false));
            
            isDisplayed = false;
        }
        
        public void HideInstantly()
        {
            if (!isDisplayed)
                return;

            StopAllCoroutines();
            border.gameObject.SetActive(false);
            text.gameObject.SetActive(false);
            
            isDisplayed = false;
        }
        
        public void AnimateClick()
        {
            border.rectTransform.localScale = new Vector3(1.5f, 0.5f, 1.0f);
            StartCoroutine(Tools.TweenLocalScale(border.rectTransform, startingScale.x, startingScale.y, startingScale.z, clickAnimationDuration));
            SFXManager.instance.PlaySFX(clickSound);
        }
    }
}
