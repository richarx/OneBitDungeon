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

    private void Start()
    {
        AudioSource audioSource = SFXManager.instance.PlaySFX(audioClip, 0.0f, loop: true, pitch: -1.0f);

        currentSequence = Sequence.Create()
            .Group(Tween.AudioVolume(audioSource, 0.0f, volume, fadeInDuration, Ease.InCirc));

        GameManager.OnPrepareToChangeScene.AddListener(() =>
        {
            if (currentSequence.isAlive)
                currentSequence.Stop();

            currentSequence = Sequence.Create()
                .Group(Tween.AudioVolume(audioSource, 0.0f, 0.5f));
        });
    }
}
