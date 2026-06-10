using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CodeAnimator : MonoBehaviour
{
    public enum AnimationType
    {
        Idle,
        Walk,
    }

    public enum AnimationDirection
    {
        L,
        F,
        R,
        BR,
        B,
        BL
    }

    [SerializeField] private SpriteRenderer graphics;
    [SerializeField] private AnimationsHolderData animationsHolder;

    private AnimationType currentAnimation;
    private AnimationDirection currentDirection;
    private bool currentWeaponState;

    public void PlayAnimation(AnimationType animationType, AnimationDirection animationDirection, bool hasWeaponInHand = false)
    {
        if (animationType == currentAnimation && animationDirection == currentDirection && hasWeaponInHand == currentWeaponState)
            return;

        currentAnimation = animationType;
        currentDirection = animationDirection;
        currentWeaponState = hasWeaponInHand;

        AnimationData animationData = RetreiveAnimationData(animationType);

        StopAllCoroutines();
        StartCoroutine(PlayAnimationCoroutine(animationData, animationDirection, hasWeaponInHand));
    }

    private IEnumerator PlayAnimationCoroutine(AnimationData animationData, AnimationDirection animationDirection, bool hasWeaponInHand)
    {
        List<Sprite> sprites = animationData.GetSprites(hasWeaponInHand);
        int frameCount = sprites.Count / animationData.directionCount;
        int directionIndex = (int)animationDirection;
        int startingFrame = directionIndex * frameCount;

        if (animationData.isLooping)
        {
            while (true)
                yield return AnimateSpriteRenderer(sprites, frameCount, startingFrame, animationData.timeBetweenFrames);
        }
        else
            yield return AnimateSpriteRenderer(sprites, frameCount, startingFrame, animationData.timeBetweenFrames);
    }

    private IEnumerator AnimateSpriteRenderer(List<Sprite> sprites, int frameCount, int startingFrame, float timeBetweenFrames)
    {
        for (int i = startingFrame; i < startingFrame + frameCount; i++)
        {
            graphics.sprite = sprites[i];
            yield return new WaitForSeconds(timeBetweenFrames);
        }
    }

    private AnimationData RetreiveAnimationData(AnimationType animationType)
    {
        switch (animationType)
        {
            default:
            case AnimationType.Idle:
                return animationsHolder.Idle;
            case AnimationType.Walk:
                return animationsHolder.Walk;
        }
    }
}
