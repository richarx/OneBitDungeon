using PrimeTween;
using TMPro;
using Tools_and_Scripts;
using UnityEngine;

public class DialogueName : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textMeshProUGUI;

    private RectTransform rectTransform;

    private Sequence currentSequence;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void Display(string npc)
    {
        if (currentSequence.isAlive)
            currentSequence.Stop();

        textMeshProUGUI.text = npc;
        textMeshProUGUI.MakeTransparent();

        currentSequence = Sequence.Create()
            .Group(Tween.UIAnchoredPositionY(rectTransform, 0.0f, 0.5f, Ease.OutCirc))
            .Group(Tween.Alpha(textMeshProUGUI, 1.0f, 1.0f, startDelay: 0.3f));
    }

    public void Hide()
    {
        if (currentSequence.isAlive)
            currentSequence.Stop();

        currentSequence = Sequence.Create()
            .Group(Tween.UIAnchoredPositionY(rectTransform, 200.0f, 0.5f, Ease.InCirc))
            .Group(Tween.Alpha(textMeshProUGUI, 0.0f, 0.15f));
    }
}
