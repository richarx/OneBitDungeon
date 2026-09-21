using System;
using System.Collections;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class SpeedLines : MonoBehaviour
{
    [SerializeField] private Image lineImage;
    [SerializeField] private List<Sprite> linesSprites;
    [SerializeField] private float timeBetweenFrames;

    private bool isDisplayed;
    private Sequence currentSequence;

    private float lastSpriteTimestamp;
    private int currentLine;

    private void Update()
    {
        if (isDisplayed)
            CycleLineSprite();
    }

    private void CycleLineSprite()
    {
        if (Time.time - lastSpriteTimestamp < timeBetweenFrames)
            return;

        lineImage.sprite = linesSprites[currentLine];
        lastSpriteTimestamp = Time.time;

        currentLine += 1;
        if (currentLine >= linesSprites.Count)
            currentLine = 0;
    }

    public void DisplayLines(float apparitionDuration, float delay = 0.0f)
    {
        if (isDisplayed)
            return;

        isDisplayed = true;
        lastSpriteTimestamp = Time.time;
        currentLine = 0;

        if (currentSequence.isAlive)
            currentSequence.Stop();

        currentSequence = Sequence.Create();

        if (delay > 0.0f)
            currentSequence.ChainDelay(delay);

        currentSequence
            .Chain(Tween.Alpha(lineImage, 0.1f, apparitionDuration));
    }

    public void HideLines(float disappearanceDuration, float delay = 0.0f)
    {
        if (!isDisplayed)
            return;

        isDisplayed = false;

        if (currentSequence.isAlive)
            currentSequence.Stop();

        currentSequence = Sequence.Create();

        if (delay > 0.0f)
            currentSequence.ChainDelay(delay);

        currentSequence
            .Chain(Tween.Alpha(lineImage, 0.0f, disappearanceDuration));
    }
}
