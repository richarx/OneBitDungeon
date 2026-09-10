using System.Collections;
using System.Collections.Generic;
using Febucci.TextAnimatorForUnity;
using Game_Manager;
using SFX;
using TMPro;
using Tools_and_Scripts;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Intro
{
    public class Intro : MonoBehaviour
    {
        [SerializeField] private Image blackScreen;
        [SerializeField] private Image trueBlackScreen;
        [Space]
        [SerializeField] private TextMeshProUGUI text;

        [Space]
        [SerializeField] private Image kingImage;
        [SerializeField] private Image throneImage;
        [SerializeField] private Image townImage;
        [SerializeField] private Image crownImage;

        [Space]
        [SerializeField] private List<AudioClip> typingSounds;
        [SerializeField] private AudioClip introMusic;
        [SerializeField] private float introMusicVolume;

        private InputPacker inputPacker = new InputPacker();

        private IEnumerator Start()
        {
            text.gameObject.GetComponent<TypewriterComponent>().onCharacterVisible.AddListener((c) => SFXManager.instance.PlayRandomSFX(typingSounds));
            blackScreen.gameObject.SetActive(true);
            trueBlackScreen.gameObject.SetActive(true);
            text.text = "";

            SFXManager.instance.PlaySFX(introMusic, introMusicVolume);

            yield return new WaitForSeconds(1.0f);
            yield return Tools.Fade(trueBlackScreen, 1.0f, false);
            yield return Tools.Fade(blackScreen, 1.5f, false);

            yield return KingIntro();
            yield return ThroneIntro();
            yield return TownIntro();
            yield return CrownIntro();

            yield return Tools.Fade(blackScreen, 2.0f, true);
            yield return Tools.Fade(trueBlackScreen, 1.0f, true);
            GoToFirstLevel();
        }

        private IEnumerator KingIntro()
        {
            Coroutine kingFade = StartCoroutine(Tools.Fade(kingImage, 3.0f, true));

            yield return new WaitForSeconds(1.0f);

            text.text = "His Supreme Condescension,\nThe King of Hubris is dead...";
            yield return WaitForInput();
            text.text = "";

            StopCoroutine(kingFade);
            yield return Tools.Fade(kingImage, 0.3f, false, 0.01f);
        }

        private IEnumerator ThroneIntro()
        {
            Coroutine throneFade = StartCoroutine(Tools.Fade(throneImage, 3.0f, true));

            yield return new WaitForSeconds(1.0f);

            text.text = "The High Throne of Haughtiness is now vacant...";
            yield return WaitForInput();
            text.text = "";

            StopCoroutine(throneFade);
            yield return Tools.Fade(throneImage, 0.3f, false, 0.01f);
        }

        private IEnumerator TownIntro()
        {
            Coroutine townFade = StartCoroutine(Tools.Fade(townImage, 3.0f, true));

            yield return new WaitForSeconds(1.0f);

            text.text = "The most arrogant knights and warriors from the four corners of the realm...";
            yield return WaitForInput();
            text.text = "Are starting their pilgrimage to reach the Capital city of Egomaniopolis...";
            yield return WaitForInput();
            text.text = "";

            StopCoroutine(townFade);
            yield return Tools.Fade(townImage, 0.3f, false, 0.01f);
        }

        private IEnumerator CrownIntro()
        {
            Coroutine crownFade = StartCoroutine(Tools.Fade(crownImage, 3.0f, true));

            yield return new WaitForSeconds(1.0f);

            text.text = "Only the most arrogant shall claim the Crown of Blatant disdain...";
            yield return WaitForInput();
            text.text = "";

            StopCoroutine(crownFade);
            yield return Tools.Fade(crownImage, 0.3f, false, 0.01f);
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

        private void GoToFirstLevel()
        {
            GameManager.instance.SetMenuState(false);
            SceneManager.LoadScene("TreeRoom");
        }
    }
}
