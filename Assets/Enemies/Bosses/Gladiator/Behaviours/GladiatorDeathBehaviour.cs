using System;
using Enemies.Scripts.Behaviours;
using Game_Manager;
using PrimeTween;
using UnityEngine;

[Serializable]
public sealed class GladiatorDeathBehaviour : IEnemyBehaviour
{
    private const float FadeDuration = 0.35f;

    [NonSerialized] private Sequence deathSequence;
    [NonSerialized] private bool fadeCompleted;
    [NonSerialized] private bool unlockSent;

    public void StartBehaviour(EnemyController enemy, BehaviourExecution execution)
    {
        if (deathSequence.isAlive)
            deathSequence.Stop();

        fadeCompleted = false;
        unlockSent = false;
        enemy.DeactivateHitbox();
        enemy.afterImage?.Cancel();

        if (enemy.Sprite == null || enemy.shadowSprite == null)
        {
            Debug.LogError($"[{enemy.name}] Gladiator death could not fade the boss because a sprite reference is missing.", enemy);
            fadeCompleted = true;
            UnlockOnce();
            return;
        }

        deathSequence = Sequence.Create()
            .Group(Tween.Alpha(enemy.Sprite, 0.0f, FadeDuration))
            .Group(Tween.Alpha(enemy.shadowSprite, 0.0f, FadeDuration))
            .ChainCallback(() =>
            {
                fadeCompleted = true;
                ForceInvisible(enemy);
                UnlockOnce();
            });
    }

    public void UpdateBehaviour(EnemyController enemy)
    {
        if (fadeCompleted)
            ForceInvisible(enemy);
    }

    public void FixedUpdateBehaviour(EnemyController enemy)
    {
    }

    public void StopBehaviour(EnemyController enemy)
    {
    }

    public void CancelBehaviour(EnemyController enemy)
    {
        if (deathSequence.isAlive)
            deathSequence.Stop();

        if (fadeCompleted)
            ForceInvisible(enemy);
    }

    public void SetSubBehaviourState(bool state)
    {
    }

    private void ForceInvisible(EnemyController enemy)
    {
        SetInvisible(enemy.Sprite);
        SetInvisible(enemy.shadowSprite);
    }

    private static void SetInvisible(SpriteRenderer sprite)
    {
        if (sprite == null)
            return;

        Color color = sprite.color;
        color.a = 0.0f;
        sprite.color = color;
        sprite.enabled = false;
    }

    private void UnlockOnce()
    {
        if (unlockSent)
            return;

        unlockSent = true;
        GameManager.OnUnlockLevel?.Invoke();
    }
}
