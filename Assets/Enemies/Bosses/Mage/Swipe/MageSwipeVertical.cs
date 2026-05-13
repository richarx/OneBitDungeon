using System.Collections.Generic;
using Enemies.Scripts.Behaviours;
using PrimeTween;
using UnityEngine;

public class MageSwipeVertical : MonoBehaviour, IEnemyBehaviour
{
    [SerializeField] private MageSwipeSpell mageSwipeSpellPrefab;
    [SerializeField] private MageData mageData;

    private bool isSubBehaviour;

    private Sequence attackSequence;

    private List<MageSwipeSpell> spells = new List<MageSwipeSpell>();

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
                .Group(CastSwipeSpell(new Vector3(8.92f, 0.0f, 10.0f), Vector2.down, 0.0f))
                .Group(CastSwipeSpell(new Vector3(4.42f, 0.0f, -10.0f), Vector2.up, 0.05f))
                .Group(CastSwipeSpell(new Vector3(0.0f, 0.0f, 10.0f), Vector2.down, 0.1f))
                .Group(CastSwipeSpell(new Vector3(-4.58f, 0.0f, -10.0f), Vector2.up, 0.15f))
                .Group(CastSwipeSpell(new Vector3(-9.11f, 0.0f, 10.0f), Vector2.down, 0.2f))
                .ChainDelay(isSecondPhase ? mageData.swipeRecoveryDuration_2 : mageData.swipeRecoveryDuration)
                .ChainCallback(() => enemy.SelectNewBehaviour());
        }
        else
        {
            attackSequence = Sequence.Create()
                .Group(CastSwipeSpell(new Vector3(8.92f, 0.0f, 10.0f), Vector2.down, 0.0f))
                .Group(CastSwipeSpell(new Vector3(4.42f, 0.0f, -10.0f), Vector2.up, 0.05f))
                .Group(CastSwipeSpell(new Vector3(0.0f, 0.0f, 10.0f), Vector2.down, 0.1f))
                .Group(CastSwipeSpell(new Vector3(-4.58f, 0.0f, -10.0f), Vector2.up, 0.15f))
                .Group(CastSwipeSpell(new Vector3(-9.11f, 0.0f, 10.0f), Vector2.down, 0.2f));
        }
    }

    private Sequence CastSwipeSpell(Vector3 position, Vector2 direction, float delay)
    {
        return Sequence.Create()
            .ChainDelay(delay)
            .ChainCallback(() =>
            {
                MageSwipeSpell spell = Instantiate(mageSwipeSpellPrefab, position, Quaternion.Euler(new Vector3(90.0f, 0.0f, 0.0f)));
                spell.Setup(direction, mageData.swipeSpawnDuration, mageData.swipeFillDuration);
                spells.Add(spell);
            });
    }

    private Sequence MoveMageToPosition(EnemyController enemy, Vector3 enemyPosition)
    {
        bool isSecondPhase = enemy.currentPhase > 0;
        float moveDuration = isSecondPhase ? mageData.swipeMoveDuration_p2 : mageData.swipeMoveDuration;

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

        foreach (MageSwipeSpell spell in spells)
            spell.Cancel();
    }

    public void SetSubBehaviourState(bool state)
    {
        isSubBehaviour = state;
    }
}
