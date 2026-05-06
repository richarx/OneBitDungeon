using PrimeTween;
using UnityEngine;

public class CinematicBlackBars : MonoBehaviour
{
    [SerializeField] RectTransform topBar;
    [SerializeField] RectTransform botBar;

    [Space]
    [SerializeField] float transitionInDuration;
    [SerializeField] Ease transitionInEase;

    [Space]
    [SerializeField] float transitionOutDuration;
    [SerializeField] Ease transitionOutEase;

    private Sequence currentSequence;

    public void Display()
    {
        if (currentSequence.isAlive)
            currentSequence.Stop();

        currentSequence = Sequence.Create()
            .Group(Tween.UISizeDelta(topBar, new Vector2(1920.0f, 0.0f), new Vector2(1920.0f, 200.0f), transitionInDuration, transitionInEase))
            .Group(Tween.UISizeDelta(botBar, new Vector2(1920.0f, 0.0f), new Vector2(1920.0f, 200.0f), transitionInDuration, transitionInEase));
    }

    public void Hide()
    {
        if (currentSequence.isAlive)
            currentSequence.Stop();

        currentSequence = Sequence.Create()
            .Group(Tween.UISizeDelta(topBar, new Vector2(1920.0f, 0.0f), transitionOutDuration, transitionOutEase))
            .Group(Tween.UISizeDelta(botBar, new Vector2(1920.0f, 0.0f), transitionOutDuration, transitionOutEase));
    }
}
