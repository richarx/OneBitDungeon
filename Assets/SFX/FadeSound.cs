using UnityEngine;

namespace SFX
{
    public class FadeSound : MonoBehaviour
    {
        private AudioSource audioSource;
        private Coroutine fadeCoroutine = null;
        
        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
        }

        public void Trigger(float duration)
        {
            if (fadeCoroutine == null)
                fadeCoroutine = StartCoroutine(Tools.FadeVolume(audioSource, duration));
        }
    }
}
