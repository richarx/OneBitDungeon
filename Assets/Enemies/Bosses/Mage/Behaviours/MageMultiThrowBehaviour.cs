using System;
using System.Collections.Generic;
using Enemies.Scripts.Behaviours;
using PrimeTween;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

[Serializable]
public sealed class MageMultiThrowBehaviour : IEnemyBehaviour
{
    [OdinSerialize] private float rotationDampening;
    [OdinSerialize] private float rockMovementDuration;
    [OdinSerialize, Required] private MageThrowSpell mageThrowSpellPrefab;

    [OdinSerialize, MinValue(0f), LabelText("Durée de déplacement centre")]
    private float multiThrowMoveDuration = 0.3f;

    [OdinSerialize, MinValue(0f), LabelText("Durée de déplacement à droite")]
    private float multiThrowMoveDurationToRight = 0.1f;

    [OdinSerialize, MinValue(0f), LabelText("Durée de déplacement à gauche")]
    private float multiThrowMoveDurationToLeft = 0.1f;

    [OdinSerialize, MinValue(0f), LabelText("Durée de spawn")]
    private float multiThrowSpawnDuration = 0.5f;

    [OdinSerialize, MinValue(0f), LabelText("Durée de Fill")]
    private float multiThrowFillDuration = 0.5f;

    [OdinSerialize, MinValue(0f), LabelText("Durée de rotation")]
    private float multiThrowRotationDuration = 1f;

    [OdinSerialize, MinValue(0f), LabelText("Durée de Recovery")]
    private float multiThrowRecoveryDuration = 0.3f;

    [NonSerialized] private Sequence attackSequence;
    [NonSerialized] private List<MageThrowSpell> spells = new List<MageThrowSpell>();

    public MageMultiThrowBehaviour()
    {
    }

    public MageMultiThrowBehaviour(
        float rotationDampening,
        float rockMovementDuration,
        MageThrowSpell mageThrowSpellPrefab,
        float multiThrowMoveDuration,
        float multiThrowMoveDurationToRight,
        float multiThrowMoveDurationToLeft,
        float multiThrowSpawnDuration,
        float multiThrowFillDuration,
        float multiThrowRotationDuration,
        float multiThrowRecoveryDuration)
    {
        this.rotationDampening = rotationDampening;
        this.rockMovementDuration = rockMovementDuration;
        this.mageThrowSpellPrefab = mageThrowSpellPrefab;
        this.multiThrowMoveDuration = multiThrowMoveDuration;
        this.multiThrowMoveDurationToRight = multiThrowMoveDurationToRight;
        this.multiThrowMoveDurationToLeft = multiThrowMoveDurationToLeft;
        this.multiThrowSpawnDuration = multiThrowSpawnDuration;
        this.multiThrowFillDuration = multiThrowFillDuration;
        this.multiThrowRotationDuration = multiThrowRotationDuration;
        this.multiThrowRecoveryDuration = multiThrowRecoveryDuration;
    }

    public void StartBehaviour(EnemyController enemy, BehaviourExecution execution)
    {
        ResetRuntimeState();
        Debug.Log("Mage THROW");

        Vector3 center = new Vector3(0.0f, 0.0f, 8.5f);
        Vector3 right = new Vector3(6.0f, 0.0f, 8.5f);
        Vector3 left = new Vector3(-6.0f, 0.0f, 8.5f);

        attackSequence = Sequence.Create()
            .Chain(MoveMageToPosition(enemy, center, multiThrowMoveDuration))
            .Chain(ShootRock(enemy, center + Vector3.right * 3.0f, 0.0f, true))
            .Group(ShootRock(enemy, center - Vector3.right * 3.0f, 0.5f, false))
            .ChainDelay(1.85f)
            .Chain(MoveMageToPosition(enemy, right, multiThrowMoveDurationToRight))
            .Chain(ShootRock(enemy, right + Vector3.right * 3.0f, 0.0f, true))
            .Group(ShootRock(enemy, right - Vector3.right * 3.0f, 0.5f, false))
            .ChainDelay(1.65f)
            .Chain(MoveMageToPosition(enemy, left, multiThrowMoveDurationToLeft))
            .Chain(ShootRock(enemy, left + Vector3.right * 3.0f, 0.0f, true))
            .Group(ShootRock(enemy, left - Vector3.right * 3.0f, 0.5f, false))
            .ChainDelay(1.65f)
            .ChainDelay(multiThrowRecoveryDuration)
            .ChainCallback(() => execution.Complete());
    }
    public void UpdateBehaviour(EnemyController enemy) { }
    public void FixedUpdateBehaviour(EnemyController enemy) { }
    public void StopBehaviour(EnemyController enemy) { }
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
                    multiThrowRotationDuration,
                    rotationDampening,
                    rockMovementDuration,
                    multiThrowSpawnDuration,
                    multiThrowFillDuration,
                    () => enemy.animator.Play(isRight ? "Shoot_Right" : "Shoot_Left"));
                spells.Add(spell);
            });
    }

    private Sequence MoveMageToPosition(EnemyController enemy, Vector3 position, float duration)
    {
        return Sequence.Create()
            .ChainCallback(() =>
            {
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
