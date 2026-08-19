using PrimeTween;
using UnityEngine;

public class CinematicBlackBars : MonoBehaviour
{
    [SerializeField] RectTransform topBar;
    [SerializeField] RectTransform botBar;

    private Sequence currentSequence;

    public void Display(float duration, Ease ease, float size)
    {
        if (currentSequence.isAlive)
            currentSequence.Stop();

        currentSequence = Sequence.Create()
            .Group(Tween.UISizeDelta(topBar, new Vector2(1920.0f, 0.0f), new Vector2(1920.0f, size), duration, ease))
            .Group(Tween.UISizeDelta(botBar, new Vector2(1920.0f, 0.0f), new Vector2(1920.0f, size), duration, ease));
    }

    public void Hide(float duration, Ease ease)
    {
        if (currentSequence.isAlive)
            currentSequence.Stop();

        currentSequence = Sequence.Create()
            .Group(Tween.UISizeDelta(topBar, new Vector2(1920.0f, 0.0f), duration, ease))
            .Group(Tween.UISizeDelta(botBar, new Vector2(1920.0f, 0.0f), duration, ease));
    }
}
