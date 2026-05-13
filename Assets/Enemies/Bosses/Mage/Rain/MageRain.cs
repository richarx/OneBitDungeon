using Enemies.Scripts.Behaviours;
using PrimeTween;
using UnityEngine;

public class MageRain : MonoBehaviour, IEnemyBehaviour
{
    [SerializeField] private MageRainSpell mageRainSpellPrefab;
    [SerializeField] private MageData mageData;

    private bool isSubBehaviour;

    private Sequence attackSequence;
    private MageRainSpell spell;

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
            .ChainDelay(isSecondPhase ? mageData.rainRecoveryDuration_p2 : mageData.rainRecoveryDuration)
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
                spell = Instantiate(mageRainSpellPrefab, Vector3.zero, Quaternion.Euler(new Vector3(90.0f, 0.0f, 0.0f)));
                spell.Setup(0.1f, mageData.rainSpawnDuration, mageData.rainFillDuration);
            });
    }

    private Sequence MoveMageToPosition(EnemyController enemy, Vector3 enemyPosition)
    {
        bool isSecondPhase = enemy.currentPhase > 0;
        float moveDuration = isSecondPhase ? mageData.rainMoveDuration_p2 : mageData.rainMoveDuration;

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

        if (spell != null)
            spell.Cancel();
    }

    public void SetSubBehaviourState(bool state)
    {
        isSubBehaviour = state;
    }
}
