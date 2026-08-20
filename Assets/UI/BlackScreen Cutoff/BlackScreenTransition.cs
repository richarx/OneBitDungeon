using System.Collections;
using PrimeTween;
using Tools_and_Scripts;
using UnityEngine;
using UnityEngine.UI;

public class BlackScreenTransition : MonoBehaviour
{
    [SerializeField] private Image blackScreen;
    [SerializeField] private RectTransform mask;
    [SerializeField] private Image maskedScreen;

    public IEnumerator OpenCircle(Vector3 worldPosition, float duration)
    {
        HideInstant();

        Vector2 screenPosition = CamerasHolder.instance.mainCamera.WorldToScreenPoint(worldPosition);
        mask.position = screenPosition;

        maskedScreen.SetMaterialDirty();

        Sequence sequence = Sequence.Create()
            .Chain(Tween.UISizeDelta(mask, Vector2.zero, Vector2.one * 2500.0f, duration, Ease.InQuad));

        yield return new WaitWhile(() => sequence.isAlive);
    }

    private void CloseCircleInstant()
    {
        mask.sizeDelta = Vector2.zero;
    }

    private void OpenCircleInstant()
    {
        mask.sizeDelta = Vector2.one * 2500.0f;
    }

    public void DisplayInstant()
    {
        Tools.SetImageAlpha(blackScreen, 1.0f);
    }

    public void HideInstant()
    {
        Tools.SetImageAlpha(blackScreen, 0.0f);
    }

    public IEnumerator FadeIn(float duration, bool useScaledTime = true)
    {
        yield return FadeBlackScreen(duration, true, useScaledTime);
    }

    public IEnumerator FadeOut(float duration, bool useScaledTime = true)
    {
        yield return FadeBlackScreen(duration, false, useScaledTime);
    }

    private IEnumerator FadeBlackScreen(float duration, bool fadeIn, bool useScaledTime)
    {
        OpenCircleInstant();

        float startingAlpha = fadeIn ? 0.0f : 1.0f;
        float targetAlpha = fadeIn ? 1.0f : 0.0f;

        Sequence sequence = Sequence.Create(useUnscaledTime: !useScaledTime)
            .Chain(Tween.Alpha(blackScreen, startingAlpha, targetAlpha, duration, useUnscaledTime: !useScaledTime));

        yield return new WaitWhile(() => sequence.isAlive);
    }
}
