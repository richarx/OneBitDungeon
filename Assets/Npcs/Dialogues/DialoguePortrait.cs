using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class DialoguePortrait : MonoBehaviour
{
    [SerializeField] private Image portraitImage;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private Sequence currentSequence;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Display(Sprite npcSprite)
    {
        if (currentSequence.isAlive)
            currentSequence.Stop();

        portraitImage.sprite = npcSprite;

        currentSequence = Sequence.Create()
            .Group(Tween.UIAnchoredPositionX(rectTransform, 0.0f, 0.5f, Ease.OutBack))
            .Group(Tween.Alpha(canvasGroup, 0.0f, 1.0f, 1.5f));
    }

    public void Hide()
    {
        if (currentSequence.isAlive)
            currentSequence.Stop();

        currentSequence = Sequence.Create()
            .Group(Tween.UIAnchoredPositionX(rectTransform, 480.0f, 0.5f, Ease.InBack))
            .Group(Tween.Alpha(canvasGroup, 0.0f, 0.5f));
    }
}
