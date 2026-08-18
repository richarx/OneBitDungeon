using System;
using System.Collections.Generic;
using Enemies.Scripts.Behaviours;
using PrimeTween;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

[Serializable]
public sealed class MageThrowBehaviour : IEnemyBehaviour
{
    [OdinSerialize] private float rotationDampening;
    [OdinSerialize] private float rockMovementDuration;
    [OdinSerialize, Required] private MageThrowSpell mageThrowSpellPrefab;
    [OdinSerialize, Required] private MageData mageData;

    [NonSerialized] private Sequence attackSequence;
    [NonSerialized] private List<MageThrowSpell> spells = new List<MageThrowSpell>();
    public MageThrowBehaviour()
    {
    }

    public MageThrowBehaviour(
        float rotationDampening,
        float rockMovementDuration,
        MageThrowSpell mageThrowSpellPrefab,
        MageData mageData)
    {
        this.rotationDampening = rotationDampening;
        this.rockMovementDuration = rockMovementDuration;
        this.mageThrowSpellPrefab = mageThrowSpellPrefab;
        this.mageData = mageData;
    }

    public void StartBehaviour(EnemyController enemy, BehaviourExecution execution)
    {
        ResetRuntimeState();
        Debug.Log("Mage THROW");
        Vector3 enemyPosition = new Vector3(0.0f, 0.0f, 8.5f);
        Vector3 rightPosition = new Vector3(3.0f, 0.0f, 9.0f);
        Vector3 leftPosition = new Vector3(-3.0f, 0.0f, 9.0f);
        bool isSecondPhase = enemy.currentPhase > 0;
        attackSequence = Sequence.Create()
            .Chain(MoveMageToPosition(enemy, enemyPosition))
            .Chain(ShootRock(enemy, rightPosition, 0.0f, true))
            .Group(ShootRock(enemy, leftPosition, 0.5f, false))
            .ChainDelay(isSecondPhase ? mageData.throwRecoveryDuration_p2 : mageData.throwRecoveryDuration)
            .ChainCallback(() => execution.Complete());
    }
    public void UpdateBehaviour(EnemyController enemy) { }
    public void FixedUpdateBehaviour(EnemyController enemy) { }
    public void StopBehaviour(EnemyController enemy)
    {
        if (attackSequence.isAlive)
            attackSequence.Stop();
    }

    public void CancelBehaviour(EnemyController enemy)
    {
        ResetRuntimeState();
    }

    public void SetSubBehaviourState(bool state) { }
    private Sequence ShootRock(EnemyController enemy, Vector3 position, float delay, bool isRight)
    {
        return Sequence.Create()
            .ChainDelay(delay)
            .ChainCallback(() =>
            {
                enemy.animator.Play("Cast");
                MageThrowSpell spell = UnityEngine.Object.Instantiate(
                    mageThrowSpellPrefab,
                    position,
                    Quaternion.identity);
                spell.Setup(
                    mageData.throwRotationDuration,
                    rotationDampening,
                    rockMovementDuration,
                    mageData.throwSpawnDuration,
                    mageData.throwFillDuration,
                    () => enemy.animator.Play(isRight ? "Shoot_Right" : "Shoot_Left"));
                spells.Add(spell);
            });
    }
    private Sequence MoveMageToPosition(EnemyController enemy, Vector3 position)
    {
        bool isSecondPhase = enemy.currentPhase > 0;
        float duration = isSecondPhase ? mageData.throwMoveDuration_p2 : mageData.throwMoveDuration;
        return Sequence.Create()
            .ChainCallback(() =>
            {
                if (isSecondPhase)
                    enemy.afterImage.Trigger(duration);

                MageSFX.instance.PlayMageMove();
            })
            .Group(Tween.Position(enemy.transform, position, duration, Ease.InOutCubic));
    }

    private void ResetRuntimeState()
    {
        if (spells == null)
            spells = new List<MageThrowSpell>();

        if (attackSequence.isAlive)
            attackSequence.Stop();

        foreach (MageThrowSpell spell in spells)
        {
            if (spell != null)
                spell.Cancel();
        }

        attackSequence = default;
        spells.Clear();
    }
}
