using Enemies.Scripts.Behaviours;
using PrimeTween;
using UnityEngine;

public class MageSwipeVertical : MonoBehaviour, IEnemyBehaviour
{
    [SerializeField] private MageSwipeSpell mageSwipeSpellPrefab;

    private bool isSubBehaviour;

    private Sequence attackSequence;

    private float spawnDuration = 1.0f;
    private float fillDuration = 0.5f;

    public void StartBehaviour(EnemyController enemy)
    {
        Debug.Log("Mage SWIPE VERTICAL");

        bool isSecondPhase = enemy.currentPhase > 0;

        Vector3 randomPosition = Random.insideUnitSphere * 7.0f;
        randomPosition.y = 0.0f;

        if (!isSubBehaviour)
        {
            attackSequence = Sequence.Create()
                .ChainCallback(() => enemy.animator.Play("Cast"))
                .Chain(MoveMageToPosition(enemy, randomPosition))
                .Group(CastSwipeSpell(new Vector3(8.92f, 0.0f, 10.0f), Vector2.down))
                .ChainDelay(0.05f)
                .Chain(CastSwipeSpell(new Vector3(4.42f, 0.0f, -10.0f), Vector2.up))
                .ChainDelay(0.05f)
                .Chain(CastSwipeSpell(new Vector3(0.0f, 0.0f, 10.0f), Vector2.down))
                .ChainDelay(0.05f)
                .Chain(CastSwipeSpell(new Vector3(-4.58f, 0.0f, -10.0f), Vector2.up))
                .ChainDelay(0.05f)
                .Chain(CastSwipeSpell(new Vector3(-9.11f, 0.0f, 10.0f), Vector2.down))
                .ChainDelay(isSecondPhase ? (spawnDuration + fillDuration) - 0.5f : (spawnDuration + fillDuration) + 1.0f)
                .ChainCallback(() => enemy.SelectNewBehaviour());
        }
        else
        {
            attackSequence = Sequence.Create()
                .Group(CastSwipeSpell(new Vector3(8.92f, 0.0f, 10.0f), Vector2.down))
                .ChainDelay(0.05f)
                .Chain(CastSwipeSpell(new Vector3(4.42f, 0.0f, -10.0f), Vector2.up))
                .ChainDelay(0.05f)
                .Chain(CastSwipeSpell(new Vector3(0.0f, 0.0f, 10.0f), Vector2.down))
                .ChainDelay(0.05f)
                .Chain(CastSwipeSpell(new Vector3(-4.58f, 0.0f, -10.0f), Vector2.up))
                .ChainDelay(0.05f)
                .Chain(CastSwipeSpell(new Vector3(-9.11f, 0.0f, 10.0f), Vector2.down));
        }
    }

    private Sequence CastSwipeSpell(Vector3 position, Vector2 direction)
    {
        return Sequence.Create()
            .ChainCallback(() =>
            {
                MageSwipeSpell spell = Instantiate(mageSwipeSpellPrefab, position, Quaternion.Euler(new Vector3(90.0f, 0.0f, 0.0f)));
                spell.Setup(direction, spawnDuration, fillDuration);
            });
    }

    private Sequence MoveMageToPosition(EnemyController enemy, Vector3 enemyPosition)
    {
        bool isSecondPhase = enemy.currentPhase > 0;
        float moveDuration = isSecondPhase ? 0.5f : 1.0f;

        return Sequence.Create()
            .ChainDelay(0.5f)
            .ChainCallback(() =>
            {
                if (isSecondPhase)
                    enemy.afterImage.Trigger(moveDuration);
                MageSFX.instance.PlayMageMove();
            })
            .Group(Tween.Position(enemy.transform, enemyPosition, moveDuration, Ease.InOutCubic));
    }

    public void UpdateBehaviour(EnemyController enemy)
    {
    }

    public void FixedUpdateBehaviour(EnemyController enemy)
    {
    }

    public void StopBehaviour(EnemyController enemy)
    {
        if (attackSequence.isAlive && !isSubBehaviour)
            attackSequence.Stop();
    }

    public void CancelBehaviour(EnemyController enemy)
    {
        if (attackSequence.isAlive)
            attackSequence.Stop();
    }

    public void SetSubBehaviourState(bool state)
    {
        isSubBehaviour = state;
    }
}
