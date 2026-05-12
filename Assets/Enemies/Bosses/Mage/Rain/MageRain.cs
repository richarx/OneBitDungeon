using Enemies.Scripts.Behaviours;
using PrimeTween;
using UnityEngine;

public class MageRain : MonoBehaviour, IEnemyBehaviour
{
    [SerializeField] private MageRainSpell mageRainSpellPrefab;

    private bool isSubBehaviour;

    private Sequence attackSequence;

    public void StartBehaviour(EnemyController enemy)
    {
        Vector3 randomPosition = Random.insideUnitSphere * 7.0f;
        randomPosition.y = 0.0f;

        bool isSecondPhase = enemy.currentPhase > 0;

        if (!isSubBehaviour)
        {
            enemy.animator.Play("Cast");

            attackSequence = Sequence.Create()
            .Chain(MoveMageToPosition(enemy, randomPosition))
            .Group(CastRainSpell())
            .ChainDelay(isSecondPhase ? 1.0f : 1.5f)
            .ChainCallback(() => enemy.SelectNewBehaviour());
        }
        else
        {
            CastRainSpell();
        }
    }

    private Sequence CastRainSpell()
    {
        return Sequence.Create()
            .ChainCallback(() =>
            {
                MageRainSpell spell = Instantiate(mageRainSpellPrefab, Vector3.zero, Quaternion.Euler(new Vector3(90.0f, 0.0f, 0.0f)));
                spell.Setup();
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
