using Febucci.TextAnimatorForUnity;
using Febucci.TextAnimatorForUnity.TextMeshPro;
using PrimeTween;
using TMPro;
using UnityEngine;

public class DialogueDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textMeshProUGUI;

    private RectTransform rectTransform;
    private TextAnimator_TMP textAnimator;
    private TypewriterComponent typewriter;

    private Sequence currentSequence;
    public bool isDisplayed { get; private set; }

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        textAnimator = textMeshProUGUI.GetComponent<TextAnimator_TMP>();
        typewriter = textMeshProUGUI.GetComponent<TypewriterComponent>();
    }

    public void Display(string currentLine)
    {
        if (currentSequence.isAlive)
            currentSequence.Stop();

        textMeshProUGUI.text = "";

        currentSequence = Sequence.Create()
            .Chain(Tween.UIAnchoredPositionY(rectTransform, 100.0f, 0.5f, Ease.OutCirc))
            .ChainCallback(() => textMeshProUGUI.text = currentLine)
            .ChainCallback(() => isDisplayed = true);
    }

    public void Hide()
    {
        if (currentSequence.isAlive)
            currentSequence.Stop();

        currentSequence = Sequence.Create()
            .Chain(Tween.UIAnchoredPositionY(rectTransform, -250.0f, 0.5f, Ease.InCirc))
            .ChainCallback(() => isDisplayed = false);
    }

    public void DisplayNewLine(string currentLine)
    {
        textMeshProUGUI.text = currentLine;
    }

    public bool IsLineBeingAnimated()
    {
        return typewriter.IsShowingText && !textAnimator.allLettersShown;
    }

    public void InstantlyDisplayLine()
    {
        typewriter.SkipTypewriter();
    }
}
