using System;

public static class SekiroAnimation
{
    public static void Play(EnemyController enemy, string animationName)
    {
        if (enemy == null || string.IsNullOrWhiteSpace(animationName))
            return;

        if (enemy.codeAnimator != null
            && Enum.TryParse(animationName, true, out CodeAnimator.AnimationType animationType))
        {
            enemy.codeAnimator.PlayAnimation(animationType, CodeAnimator.AnimationDirection.F, true);
            return;
        }

        if (enemy.animator != null)
            enemy.animator.Play(animationName);
    }
}
