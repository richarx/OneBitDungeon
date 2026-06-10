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
        Attack,
        ParryStart,
        ParrySuccess,
        ParryRecovery,
        Hurt,
        Die,
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

    public enum AnimationDirectionCount
    {
        Six,
        Four,
        Two
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
        if (!animationData.canSelfCancel && animationData == currentAnimation && animationDirection == currentDirection && hasWeaponInHand == currentWeaponState)
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
        int frameCount = sprites.Count / ComputeDirectionCount(animationData.directionCount);
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

    private int ComputeStartingFrame(AnimationDirectionCount directionCount, AnimationDirection animationDirection, int frameCount)
    {
        if (directionCount == AnimationDirectionCount.Four && animationDirection == AnimationDirection.B)
            return 3 * frameCount;

        if (directionCount == AnimationDirectionCount.Two && animationDirection == AnimationDirection.R)
            return frameCount;

        return (int)animationDirection * frameCount;
    }

    private int ComputeDirectionCount(AnimationDirectionCount directionCount)
    {
        switch (directionCount)
        {
            default:
            case AnimationDirectionCount.Six:
                return 6;
            case AnimationDirectionCount.Four:
                return 4;
            case AnimationDirectionCount.Two:
                return 2;
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
            case AnimationType.Jump:
                return animationsHolder.Jump;
            case AnimationType.Roll:
                return animationsHolder.Roll;
            case AnimationType.Attack:
                return animationsHolder.Attack;
            case AnimationType.ParryStart:
                return animationsHolder.ParryStart;
            case AnimationType.ParrySuccess:
                return animationsHolder.ParrySuccess;
            case AnimationType.ParryRecovery:
                return animationsHolder.ParryRecovery;
            case AnimationType.Hurt:
                return animationsHolder.Hurt;
            case AnimationType.Die:
                return animationsHolder.Die;
            case AnimationType.GetUp:
                return animationsHolder.GetUp;
            case AnimationType.SitDown:
                return animationsHolder.SitDown;
            case AnimationType.Sit:
                return animationsHolder.Sit;
        }
    }
}
