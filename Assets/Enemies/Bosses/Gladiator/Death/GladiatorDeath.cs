using Enemies.Scripts.Behaviours;
using Game_Manager;
using PrimeTween;
using UnityEngine;

public class GladiatorDeath : MonoBehaviour, IEnemyBehaviour
{
    private const float FadeDuration = 0.35f;

    public void StartBehaviour(EnemyController enemy, BehaviourExecution execution)
    {
        enemy.DeactivateHitbox();
        enemy.afterImage?.Cancel();

        if (enemy.Sprite == null || enemy.shadowSprite == null)
        {
            Debug.LogError($"[{enemy.name}] Gladiator death could not fade the boss because a sprite reference is missing.", enemy);
            GameManager.OnUnlockLevel?.Invoke();
            return;
        }

        Sequence.Create()
            .Group(Tween.Alpha(enemy.Sprite, 0.0f, FadeDuration))
            .Group(Tween.Alpha(enemy.shadowSprite, 0.0f, FadeDuration))
            .ChainCallback(() =>
            {
                enemy.Sprite.enabled = false;
                enemy.shadowSprite.enabled = false;
            })
            .ChainCallback(() => GameManager.OnUnlockLevel?.Invoke());
    }

    public void UpdateBehaviour(EnemyController enemy)
    {
    }

    public void FixedUpdateBehaviour(EnemyController enemy)
    {
    }

    public void StopBehaviour(EnemyController enemy)
    {
    }

    public void CancelBehaviour(EnemyController enemy)
    {
    }

    public void SetSubBehaviourState(bool state)
    {
    }

}
