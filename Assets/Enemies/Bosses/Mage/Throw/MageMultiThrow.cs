using System.Collections.Generic;
using Enemies.Scripts.Behaviours;
using PrimeTween;
using UnityEngine;

public class MageMultiThrow : MonoBehaviour, IEnemyBehaviour
{
    [SerializeField] private float rotationDampening;
    [SerializeField] private float rockMovementDuration;
    [SerializeField] private MageThrowSpell mageThrowSpellPrefab;
    [SerializeField] private MageData mageData;

    private Sequence attackSequence;

    private List<MageThrowSpell> spells = new List<MageThrowSpell>();

    public void StartBehaviour(EnemyController enemy)
    {
        Debug.Log("Mage THROW");

        Vector3 enemyPosition = new Vector3(0.0f, 0.0f, 8.5f);
        Vector3 enemyRightPosition = new Vector3(6.0f, 0.0f, 8.5f);
        Vector3 enemyLeftPosition = new Vector3(-6.0f, 0.0f, 8.5f);

        attackSequence = Sequence.Create()
            .Chain(MoveMageToPosition(enemy, enemyPosition, mageData.multiThrowMoveDuration))
            .Chain(ShootRock(enemy, enemyPosition + Vector3.right * 3.0f, 0.0f, true))
            .Group(ShootRock(enemy, enemyPosition - Vector3.right * 3.0f, 0.5f, false))
            .ChainDelay(1.85f)
            .Chain(MoveMageToPosition(enemy, enemyRightPosition, mageData.multiThrowMoveDuration_toRight))
            .Chain(ShootRock(enemy, enemyRightPosition + Vector3.right * 3.0f, 0.0f, true))
            .Group(ShootRock(enemy, enemyRightPosition - Vector3.right * 3.0f, 0.5f, false))
            .ChainDelay(1.65f)
            .Chain(MoveMageToPosition(enemy, enemyLeftPosition, mageData.multiThrowMoveDuration_toLeft))
            .Chain(ShootRock(enemy, enemyLeftPosition + Vector3.right * 3.0f, 0.0f, true))
            .Group(ShootRock(enemy, enemyLeftPosition - Vector3.right * 3.0f, 0.5f, false))
            .ChainDelay(1.65f)
            .ChainDelay(mageData.multiThrowRecoveryDuration)
            .ChainCallback(() => enemy.SelectNewBehaviour());
    }

    private Sequence ShootRock(EnemyController enemy, Vector3 startingPosition, float delay, bool isRight)
    {
        return Sequence.Create()
        .ChainDelay(delay)
        .ChainCallback(() =>
        {
            enemy.animator.Play("Cast");

            MageThrowSpell spell = Instantiate(mageThrowSpellPrefab, startingPosition, Quaternion.identity);
            spell.Setup(mageData.multiThrowRotationDuration, rotationDampening, rockMovementDuration, mageData.multiThrowSpawnDuration, mageData.multiThrowFillDuration, () => enemy.animator.Play(isRight ? "Shoot_Right" : "Shoot_Left"));
            spells.Add(spell);
        });
    }

    private Sequence MoveMageToPosition(EnemyController enemy, Vector3 enemyPosition, float moveDuration)
    {
        return Sequence.Create()
            .ChainCallback(() =>
            {
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
    }

    public void CancelBehaviour(EnemyController enemy)
    {
        if (attackSequence.isAlive)
            attackSequence.Stop();

        foreach (MageThrowSpell spell in spells)
            spell.Cancel();
    }

    public void SetSubBehaviourState(bool state)
    {
    }
}
