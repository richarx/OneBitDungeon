using Game_Manager;
using PrimeTween;
using SFX;
using UnityEngine;

public class AmbiantSoundPlayer : MonoBehaviour
{
    [SerializeField] private AudioClip audioClip;
    [SerializeField] private float volume;
    [SerializeField] private float fadeInDuration;

    private Sequence currentSequence;
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = SFXManager.instance.PlaySFX(audioClip, 0.0f, loop: true, pitch: -1.0f);

        currentSequence = Sequence.Create()
            .Group(Tween.AudioVolume(audioSource, 0.0f, volume, fadeInDuration, Ease.InCirc));

    }

    private void OnEnable()
    {
        GameManager.OnPrepareToChangeScene.AddListener(() => FadeOutSound(audioSource));

    }

    private void OnDisable()
    {
        GameManager.OnPrepareToChangeScene.RemoveListener(() => FadeOutSound(audioSource));

    }

    private void FadeOutSound(AudioSource audioSource)
    {
        if (currentSequence.isAlive)
            currentSequence.Stop();

        currentSequence = Sequence.Create()
            .Group(Tween.AudioVolume(audioSource, 0.0f, 0.4f));
    }
}
