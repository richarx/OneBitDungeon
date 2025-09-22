using System.Collections;
using System.Collections.Generic;
using Febucci.UI;
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
        [Space] 
        [SerializeField] private TextMeshProUGUI text;

        [Space] 
        [SerializeField] private Image dogImage;
        [SerializeField] private Image bossImage;

        [Space]
        [SerializeField] private List<AudioClip> typingSounds;
        [SerializeField] private AudioClip introMusic;
        [SerializeField] private float introMusicVolume;
        
        [Space]
        [SerializeField] private List<AudioClip> dogSounds;
        [SerializeField] private AudioClip bossSound;


        private Image dogShadow;
        private Image bossShadow;
        
        private InputPacker inputPacker = new InputPacker();
        
        private IEnumerator Start()
        {
            text.gameObject.GetComponent<TypewriterByCharacter>().onCharacterVisible.AddListener((c) => SFXManager.instance.PlayRandomSFX(typingSounds));
            blackScreen.gameObject.SetActive(true);
            text.text = "";
            dogShadow = dogImage.transform.GetChild(0).GetComponent<Image>();
            bossShadow = bossImage.transform.GetChild(0).GetComponent<Image>();

            SFXManager.instance.PlaySFX(introMusic, introMusicVolume);
            
            yield return new WaitForSeconds(1.0f);
        
            Coroutine fadeBlackScreen = StartCoroutine(Tools.Fade(blackScreen, 4.0f, false));
            yield return new WaitForSeconds(0.5f);

            yield return DogIntro();
            yield return BossIntro();
            yield return LastMessage();

            yield return Tools.Fade(blackScreen, 2.0f, true);
            GoToFirstLevel();
        }

        private IEnumerator DogIntro()
        {
            StartCoroutine(Tools.Fade(dogImage, 0.3f, true));
            yield return Tools.Fade(dogShadow, 0.3f, true, 0.01f);
            
            yield return new WaitForSeconds(0.5f);
            SFXManager.instance.PlaySFX(dogSounds[0]);
            SFXManager.instance.PlaySFX(dogSounds[1], delay:0.2f);
            SFXManager.instance.PlaySFX(dogSounds[2], delay:0.4f);
            
            text.text = "This is your dog : \n\"Bobby John John\".";
            yield return WaitForInput();
            text.text = "You love your dog.";
            yield return WaitForInput();
            text.text = "";
            
            StartCoroutine(Tools.Fade(dogImage, 0.3f, false));
            yield return Tools.Fade(dogShadow, 0.3f, false, 0.01f);
        }

        private IEnumerator BossIntro()
        {
            yield return new WaitForSeconds(0.5f);
            
            StartCoroutine(Tools.Fade(bossImage, 0.3f, true));
            yield return Tools.Fade(bossShadow, 0.3f, true, 0.01f);
            
            yield return new WaitForSeconds(0.5f);
            SFXManager.instance.PlaySFX(bossSound);

            text.text = "This is Blazgul the Wicked.";
            yield return WaitForInput();
            text.text = "He stole your dog !";
            yield return WaitForInput();
            text.text = "Such wickedness shall be punished.";
            yield return WaitForInput();
            StartCoroutine(Tools.Fade(bossImage, 0.3f, false));
            yield return Tools.Fade(bossShadow, 0.3f, false, 0.01f);
            text.text = "";
        }

        private IEnumerator LastMessage()
        {
            yield return new WaitForSeconds(0.5f);
            text.text = "Hold on Bobby John John !";
            yield return new WaitForSeconds(1.5f);
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
            SceneManager.LoadScene("1-1");
        }
    }
}
