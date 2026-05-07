using Febucci.TextAnimatorForUnity;
using Player.Scripts;
using PrimeTween;
using Tools_and_Scripts;
using UnityEngine;
using UnityEngine.UI;

public class DialogueActionButton : MonoBehaviour
{
    [SerializeField] private Image inputImage;
    [SerializeField] private Image waitImage;
    [SerializeField] private TypewriterComponent typewriter;
    [SerializeField] private DialogueDisplay dialogueDisplay;

    [Space]
    [SerializeField] private Sprite gamepadSprite;
    [SerializeField] private Sprite keyboardSprite;

    private bool isLastInputGamepad => PlayerStateMachine.instance.inputPackage.lastInputType == Tools_and_Scripts.InputType.Gamepad;

    private Sequence currentSequence;
    private float fadeDuration = 0.1f;

    private void Start()
    {
        typewriter.onTypewriterStart.AddListener(DisplayWait);
        typewriter.onTextShowed.AddListener(DisplayInput);

        HideInstant();
    }

    private void DisplayInput()
    {
        if (!dialogueDisplay.isDisplayed)
            return;

        if (currentSequence.isAlive)
            currentSequence.Stop();

        inputImage.sprite = isLastInputGamepad ? gamepadSprite : keyboardSprite;

        currentSequence = Sequence.Create()
            .Chain(Tween.Alpha(waitImage, 0.0f, fadeDuration))
            .Chain(Tween.Alpha(inputImage, 1.0f, fadeDuration));
    }

    private void DisplayWait()
    {
        if (currentSequence.isAlive)
            currentSequence.Stop();

        currentSequence = Sequence.Create()
            .Chain(Tween.PunchScale(inputImage.transform, new Vector3(1.3f, 0.7f, 1.0f), 0.2f))
            .Chain(Tween.Alpha(inputImage, 0.0f, fadeDuration))
            .Chain(Tween.Alpha(waitImage, 1.0f, fadeDuration));
    }

    private void HideInstant()
    {
        inputImage.MakeTransparent();
        waitImage.MakeTransparent();
    }

    public void Hide()
    {
        if (currentSequence.isAlive)
            currentSequence.Stop();

        currentSequence = Sequence.Create()
            .Chain(Tween.PunchScale(inputImage.transform, new Vector3(1.3f, 0.7f, 1.0f), 0.2f))
            .Group(Tween.Alpha(inputImage, 0.0f, 0.2f))
            .Group(Tween.Alpha(waitImage, 0.0f, 0.2f));
    }
}
