using System;
using System.Collections;
using System.Collections.Generic;
using PrimeTween;
using Rewired.Glyphs.UnityUI;
using TMPro;
using UnityEngine;

public class BossCriticalIconDisplay : MonoBehaviour
{
    [SerializeField] private EnemyHumility enemyHumility;
    [SerializeField] private UnityUITextMeshProGlyphHelper glyphHelper;

    private TextMeshProUGUI textMeshProUGUI;

    private bool isDisplayed;
    private Sequence displaySequence;
    private Sequence pulseSequence;

    private void Start()
    {
        textMeshProUGUI = glyphHelper.GetComponent<TextMeshProUGUI>();
        enemyHumility.GetComponent<EnemyController>().OnKillBoss.AddListener(() =>
        {
            if (isDisplayed)
                HideIcon();
        });
    }

    private void Update()
    {
        if (!isDisplayed && enemyHumility.IsFull)
            DisplayIcon();

        if (isDisplayed && !enemyHumility.IsFull)
            HideIcon();
    }

    private void DisplayIcon()
    {
        if (displaySequence.isAlive)
            displaySequence.Stop();

        displaySequence = Sequence.Create()
            .Chain(Tween.Alpha(textMeshProUGUI, 1.0f, 0.15f));

        pulseSequence = Sequence.Create(100, Sequence.SequenceCycleMode.Restart)
            .ChainDelay(0.3f)
            .Chain(Tween.PunchScale(textMeshProUGUI.transform, Vector3.one, 0.15f));

        glyphHelper.text = RewiredTool.ResolveRewiredActionTokens("{Critical}");
        isDisplayed = true;
    }

    private void HideIcon()
    {
        if (displaySequence.isAlive)
            displaySequence.Stop();

        if (pulseSequence.isAlive)
            pulseSequence.Stop();

        displaySequence = Sequence.Create()
            .Chain(Tween.Alpha(textMeshProUGUI, 0.0f, 0.5f));

        isDisplayed = false;
    }
}
