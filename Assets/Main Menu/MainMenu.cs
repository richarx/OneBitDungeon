using System.Collections;
using SFX;
using Tools_and_Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace Main_Menu
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private Image blackScreen;
        [SerializeField] private Button newGameButton;
        [SerializeField] private AudioClip startSound;
        
        private InputPacker inputPacker = new InputPacker();
        
        private IEnumerator Start()
        {
            blackScreen.gameObject.SetActive(true);
            newGameButton.HideInstantly();
            
            yield return new WaitForSeconds(1.0f);
            SFXManager.instance.PlaySFX(startSound);
        
            Coroutine fadeBlackScreen = StartCoroutine(Tools.Fade(blackScreen, 5.0f, false));
            yield return new WaitForSeconds(2.0f);
            
            DisplayButton();
            yield return WaitForInput();
            yield return AnimateButton();
            
            if (fadeBlackScreen != null)
                StopCoroutine(fadeBlackScreen);
            yield return Tools.Fade(blackScreen, 2.0f, true);
            yield return GoToIntroScene();
        }

        private void DisplayButton()
        {
            newGameButton.Display();
        }

        private IEnumerator WaitForInput()
        {
            while (true)
            {
                InputPackage inputPackage = inputPacker.ComputeInputPackage();
                if (IsInputPressed(inputPackage))
                    yield break;
                yield return null;
            }
        }

        private bool IsInputPressed(InputPackage inputPackage)
        {
            if (inputPackage.lastInputType == InputType.Gamepad)
                return inputPackage.southButton.wasPressedThisFrame || inputPackage.westButton.wasPressedThisFrame;
            else
                return inputPackage.leftMouse.wasPressedThisFrame || inputPackage.spaceKey.wasPressedThisFrame;
        }

        private IEnumerator AnimateButton()
        {
            newGameButton.AnimateClick();
            yield return new WaitForSeconds(0.15f);
            newGameButton.Hide();
        }

        private IEnumerator GoToIntroScene()
        {
            Debug.Log("Go to next scene");
            yield break;
        }
    }
}
