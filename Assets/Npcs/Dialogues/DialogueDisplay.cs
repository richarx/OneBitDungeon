using PrimeTween;
using UnityEngine;

public class DialogueDisplay : MonoBehaviour
{
    private RectTransform rectTransform;

    private Sequence currentSequence;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void Display()
    {
        if (currentSequence.isAlive)
            currentSequence.Stop();

        currentSequence = Sequence.Create()
            .Chain(Tween.UIAnchoredPositionY(rectTransform, 100.0f, 0.5f, Ease.OutBack));
    }

    public void Hide()
    {
        if (currentSequence.isAlive)
            currentSequence.Stop();

        currentSequence = Sequence.Create()
            .Chain(Tween.UIAnchoredPositionY(rectTransform, -250.0f, 0.5f, Ease.InBack));
    }
}
