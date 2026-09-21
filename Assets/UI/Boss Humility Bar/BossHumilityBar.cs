using System;
using System.Collections;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class BossHumilityBar : MonoBehaviour
{
    [SerializeField] private EnemyController enemyController;

    [Space]
    [SerializeField] private Image humilityBar;

    private EnemyHumility bossHumility;

    private Sequence updateHumilitySequence;
    private Sequence resetHumilitySequence;

    private RectTransform rectTransform;
    private Vector2 startingPosition;

    private void Start()
    {
        Assert.IsNotNull(enemyController, "EnemyController is not set in Boss Healthbar");

        bossHumility = enemyController.GetComponent<EnemyHumility>();
        bossHumility.OnUpdateHumility.AddListener(() => UpdateHumilityBar());
        bossHumility.OnResetHumility.AddListener(() => ResetHumilityBar());

        enemyController.OnSpawnBoss.AddListener(DisplayBar);
        enemyController.OnKillBoss.AddListener(HideBar);

        rectTransform = GetComponent<RectTransform>();
        startingPosition = rectTransform.anchoredPosition;

        HideInstant();
    }

    private void DisplayBar()
    {
        Sequence.Create()
            .ChainDelay(0.3f)
            .Chain(Tween.UIAnchoredPosition(rectTransform, startingPosition, 0.3f, Ease.OutBack));
    }

    private void HideBar()
    {
        Sequence.Create()
            .Chain(Tween.UIAnchoredPosition(rectTransform, startingPosition + Vector2.up * 300.0f, 0.3f, Ease.InBack));
    }

    private void HideInstant()
    {
        rectTransform.anchoredPosition = startingPosition + Vector2.up * 300.0f;
    }

    private void UpdateHumilityBar()
    {
        if (resetHumilitySequence.isAlive)
            resetHumilitySequence.Complete();

        if (updateHumilitySequence.isAlive)
            updateHumilitySequence.Stop();

        updateHumilitySequence = Sequence.Create()
            .Chain(Tween.UIFillAmount(humilityBar, bossHumility.currentHumilityNormalized, 0.2f, Ease.OutQuad));
    }

    private void ResetHumilityBar()
    {
        if (resetHumilitySequence.isAlive)
            resetHumilitySequence.Stop();

        if (updateHumilitySequence.isAlive)
            updateHumilitySequence.Stop();

        resetHumilitySequence = Sequence.Create()
            .Chain(Tween.UIFillAmount(humilityBar, 0.0f, 0.2f, Ease.OutQuad));
    }
}
