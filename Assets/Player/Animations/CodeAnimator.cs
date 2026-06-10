using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CodeAnimator : MonoBehaviour
{
    public enum AnimationType
    {
        Idle,
        Walk,
        Jump,
        Roll,
        GetUp,
        SitDown,
        Sit,
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

    private AnimationData currentAnimation;
    private AnimationDirection currentDirection;
    private bool currentWeaponState;

    public void PlayAnimation(AnimationType animationType, AnimationDirection animationDirection, bool hasWeaponInHand = false)
    {
        PlayAnimation(RetreiveAnimationData(animationType), animationDirection, hasWeaponInHand);
    }

    private void PlayAnimation(AnimationData animationData, AnimationDirection animationDirection, bool hasWeaponInHand = false)
    {
        if (animationData == currentAnimation && animationDirection == currentDirection && hasWeaponInHand == currentWeaponState)
            return;

        currentAnimation = animationData;
        currentDirection = animationDirection;
        currentWeaponState = hasWeaponInHand;

        StopAllCoroutines();
        StartCoroutine(PlayAnimationCoroutine(animationData, animationDirection, hasWeaponInHand));
    }

    private IEnumerator PlayAnimationCoroutine(AnimationData animationData, AnimationDirection animationDirection, bool hasWeaponInHand)
    {
        List<Sprite> sprites = animationData.GetSprites(hasWeaponInHand);
        int frameCount = sprites.Count / animationData.directionCount;
        int startingFrame = ComputeStartingFrame(animationData.directionCount, animationDirection, frameCount);

        if (animationData.isLooping)
        {
            while (true)
                yield return AnimateSpriteRenderer(sprites, frameCount, startingFrame, animationData.timeBetweenFrames);
        }
        else
            yield return AnimateSpriteRenderer(sprites, frameCount, startingFrame, animationData.timeBetweenFrames);

        if (animationData.nextAnimation != null)
            PlayAnimation(animationData.nextAnimation, animationDirection, hasWeaponInHand);
    }

    private IEnumerator AnimateSpriteRenderer(List<Sprite> sprites, int frameCount, int startingFrame, float timeBetweenFrames)
    {
        for (int i = startingFrame; i < startingFrame + frameCount; i++)
        {
            graphics.sprite = sprites[i];
            yield return new WaitForSeconds(timeBetweenFrames);
        }
    }

    private int ComputeStartingFrame(int directionCount, AnimationDirection animationDirection, int frameCount)
    {
        if (directionCount == 4 && animationDirection == AnimationDirection.B)
            return 3 * frameCount;

        if (directionCount == 2 && animationDirection == AnimationDirection.R)
            return frameCount;

        return (int)animationDirection * frameCount;
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
            case AnimationType.Jump:
                return animationsHolder.Jump;
            case AnimationType.Roll:
                return animationsHolder.Roll;
            case AnimationType.GetUp:
                return animationsHolder.GetUp;
            case AnimationType.SitDown:
                return animationsHolder.SitDown;
            case AnimationType.Sit:
                return animationsHolder.Sit;
        }
    }
}
