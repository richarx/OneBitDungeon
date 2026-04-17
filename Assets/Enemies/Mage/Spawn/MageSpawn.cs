using Enemies.Scripts.Behaviours;
using PrimeTween;
using UnityEngine;

public class MageSpawn : MonoBehaviour, IEnemyBehaviour
{
    public void FixedUpdateBehaviour(EnemyController enemy)
    {
    }

    public void StartBehaviour(EnemyController enemy)
    {
        enemy.shadowSprite.gameObject.SetActive(false);
        enemy.sprite.gameObject.SetActive(false);

        Sequence.Create()
            .ChainDelay(3.0f)
            .ChainCallback(() => enemy.shadowSprite.gameObject.SetActive(true))
            .Chain(Tween.Alpha(enemy.shadowSprite, 1.0f, 1.0f))
            .Group(Tween.Scale(enemy.shadowSprite.transform, new Vector3(0.1f, 0.1f, 1.0f), Vector3.one, 1.0f))
            .ChainCallback(() => enemy.sprite.gameObject.SetActive(true))
            .Chain(Tween.LocalPositionY(enemy.sprite.transform, 30.0f, 0.0f, 0.5f, Ease.OutBounce));
    }

    public void StopBehaviour(EnemyController enemy)
    {
    }

    public void UpdateBehaviour(EnemyController enemy)
    {
    }
}
