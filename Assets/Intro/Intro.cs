using System.Collections;
using TMPro;
using Tools_and_Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace Intro
{
    public class Intro : MonoBehaviour
    {
        [SerializeField] private Image blackScreen;
        [Space] 
        [SerializeField] private TextMeshProUGUI text;

        [Space] 
        [SerializeField] private Image dogImage;
        [SerializeField] private Image bossImage;

        private Image dogShadow;
        private Image bossShadow;
        
        private InputPacker inputPacker = new InputPacker();
        
        private IEnumerator Start()
        {
            blackScreen.gameObject.SetActive(true);
            text.text = "";
            dogShadow = dogImage.transform.GetChild(0).GetComponent<Image>();
            bossShadow = bossImage.transform.GetChild(0).GetComponent<Image>();
            yield return new WaitForSeconds(1.0f);
        
            Coroutine fadeBlackScreen = StartCoroutine(Tools.Fade(blackScreen, 4.0f, false));
            yield return new WaitForSeconds(0.5f);

            yield return DogIntro();
            
            
            
            yield return Tools.Fade(blackScreen, 2.0f, true);
            yield return GoToFirstLevel();
        }

        private IEnumerator DogIntro()
        {
            StartCoroutine(Tools.Fade(dogImage, 0.3f, true));
            yield return Tools.Fade(dogShadow, 0.3f, true, 0.01f);
            
            yield return new WaitForSeconds(0.5f);

            text.text = "This is your dog : \n\"Bobby John John\".";
            yield return WaitForInput();
            text.text = "You love your dog.";
            yield return WaitForInput();
            text.text = "";
            
            StartCoroutine(Tools.Fade(dogImage, 0.3f, false));
            yield return Tools.Fade(dogShadow, 0.3f, false, 0.01f);
        }

        private IEnumerator WaitForInput()
        {
            while (true)
            {
                yield return null;
         
                InputPackage inputPackage = inputPacker.ComputeInputPackage();
                if (IsInputPressed(inputPackage))
                    yield break;
            }
        }
        
        private bool IsInputPressed(InputPackage inputPackage)
        {
            if (inputPackage.lastInputType == InputType.Gamepad)
                return inputPackage.southButton.wasPressedThisFrame || inputPackage.westButton.wasPressedThisFrame;
            else
                return inputPackage.leftMouse.wasPressedThisFrame || inputPackage.spaceKey.wasPressedThisFrame;
        }

        private IEnumerator GoToFirstLevel()
        {
            Debug.Log("Go to First Level");
            yield break;
        }
    }
}
