using System;
using PrimeTween;
using TMPro;
using UnityEngine;

public class DialogueDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textMeshProUGUI;

    private RectTransform rectTransform;

    private Sequence currentSequence;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void Display(string currentLine)
    {
        if (currentSequence.isAlive)
            currentSequence.Stop();

        textMeshProUGUI.text = currentLine;

        currentSequence = Sequence.Create()
            .Chain(Tween.UIAnchoredPositionY(rectTransform, 100.0f, 0.5f, Ease.OutCirc));
    }

    public void Hide()
    {
        if (currentSequence.isAlive)
            currentSequence.Stop();

        currentSequence = Sequence.Create()
            .Chain(Tween.UIAnchoredPositionY(rectTransform, -250.0f, 0.5f, Ease.OutCirc));
    }

    public void DisplayNewLine(string currentLine)
    {
        textMeshProUGUI.text = currentLine;
    }

    public bool IsLineBeingAnimated()
    {
        return false;
    }

    public void InstantlyDisplayLine()
    {
    }
}
