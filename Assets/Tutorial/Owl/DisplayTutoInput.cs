using System.Collections;
using System.Collections.Generic;
using PrimeTween;
using Rewired.Glyphs.UnityUI;
using TMPro;
using Tools_and_Scripts;
using UnityEngine;
using UnityEngine.UI;

public class DisplayTutoInput : MonoBehaviour
{
    [SerializeField] private string inputText;
    [SerializeField] private float delay;
    [SerializeField] private UnityUITextMeshProGlyphHelper glyphHelper;
    [SerializeField] private Image speechBubble;

    private TextMeshProUGUI textMeshProUGUI;

    private void Start()
    {
        textMeshProUGUI = glyphHelper.GetComponent<TextMeshProUGUI>();

        glyphHelper.text = RewiredTool.ResolveRewiredActionTokens(inputText);
        textMeshProUGUI.alpha = 0.0f;
        speechBubble.MakeTransparent();

        RectTransform bubbleRect = speechBubble.GetComponent<RectTransform>();
        Vector2 size = bubbleRect.sizeDelta;
        bubbleRect.sizeDelta = new Vector2(size.x, 0.0f);

        Sequence.Create()
            .ChainDelay(delay)
            .Chain(Tween.Alpha(speechBubble, 1.0f, 0.1f))
            .Group(Tween.UISizeDelta(bubbleRect, size, 0.3f, Ease.OutBack))
            .Chain(Tween.Alpha(textMeshProUGUI, 1.0f, 0.15f));
    }
}
