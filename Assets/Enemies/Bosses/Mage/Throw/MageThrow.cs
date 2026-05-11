using Enemies.Scripts.Behaviours;
using PrimeTween;
using UnityEngine;

public class MageThrow : MonoBehaviour, IEnemyBehaviour
{
    [SerializeField] private float rotationDampening;
    [SerializeField] private float rotationDuration;
    [SerializeField] private float rockMovementDuration;
    [SerializeField] private MageThrowSpell mageThrowSpellPrefab;

    private bool isSubBehaviour;

    private Sequence attackSequence;

    public void StartBehaviour(EnemyController enemy)
    {
        Debug.Log("Mage THROW");

        Vector3 enemyPosition = new Vector3(0.0f, 0.0f, 8.5f);
        Vector3 rightPosition = new Vector3(3.0f, 0.0f, 9.0f);
        Vector3 leftPosition = new Vector3(-3.0f, 0.0f, 9.0f);

        attackSequence = Sequence.Create()
            .Chain(MoveMageToPosition(enemy, enemyPosition))
            .Chain(ShootRock(enemy, rightPosition, 0.0f, true))
            .Group(ShootRock(enemy, leftPosition, 0.5f, false))
            .ChainCallback(() => enemy.SelectNewBehaviour());
    }

    private Sequence ShootRock(EnemyController enemy, Vector3 startingPosition, float delay, bool isRight)
    {
        float delayBeforeShoot = 1.5f;

        return Sequence.Create()
        .ChainDelay(delay)
        .ChainCallback(() =>
        {
            enemy.animator.Play("Cast");

            MageThrowSpell spell = Instantiate(mageThrowSpellPrefab, startingPosition, Quaternion.identity);
            spell.Setup(rotationDuration, rotationDampening, delayBeforeShoot, rockMovementDuration, () => enemy.animator.Play(isRight ? "Shoot_Right" : "Shoot_Left"));
        });
    }

    private Sequence MoveMageToPosition(EnemyController enemy, Vector3 enemyPosition)
    {
        bool isSecondPhase = enemy.currentPhase > 0;
        float moveDuration = isSecondPhase ? 0.5f : 1.0f;

        return Sequence.Create()
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
        if (attackSequence.isAlive)
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
