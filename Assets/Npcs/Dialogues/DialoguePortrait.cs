using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class DialoguePortrait : MonoBehaviour
{
    [SerializeField] private Image portraitImage;

    private RectTransform rectTransform;

    private Sequence currentSequence;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void Display(Sprite npcSprite)
    {
        if (currentSequence.isAlive)
            currentSequence.Stop();

        portraitImage.sprite = npcSprite;

        currentSequence = Sequence.Create()
            .Group(Tween.UIAnchoredPositionX(rectTransform, 0.0f, 0.5f, Ease.OutCirc))
            .Group(Tween.Alpha(portraitImage, 0.0f, 1.0f, 1.5f));
    }

    public void Hide()
    {
        if (currentSequence.isAlive)
            currentSequence.Stop();

        currentSequence = Sequence.Create()
            .Group(Tween.UIAnchoredPositionX(rectTransform, 480.0f, 0.5f, Ease.InCirc))
            .Group(Tween.Alpha(portraitImage, 0.0f, 0.5f));
    }
}
