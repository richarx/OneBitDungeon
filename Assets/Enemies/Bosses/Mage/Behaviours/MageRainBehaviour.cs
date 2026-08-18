using System;
using Enemies.Scripts.Behaviours;
using PrimeTween;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

[Serializable]
public sealed class MageRainBehaviour : IEnemyBehaviour
{
    [OdinSerialize, Required] private MageRainSpell mageRainSpellPrefab;

    [OdinSerialize, MinValue(0f), LabelText("Durée de déplacement")]
    private float rainMoveDuration = 1f;

    [OdinSerialize, MinValue(0f), LabelText("Durée de spawn")]
    private float rainSpawnDuration = 0.5f;

    [OdinSerialize, MinValue(0f), LabelText("Durée de Fill")]
    private float rainFillDuration = 0.3f;

    [OdinSerialize, MinValue(0f), LabelText("Durée de Recovery")]
    private float rainRecoveryDuration = 0.5f;

    [NonSerialized] private bool isSubBehaviour;
    [NonSerialized] private Sequence attackSequence;
    [NonSerialized] private MageRainSpell spell;
    public MageRainBehaviour()
    {
    }

    public MageRainBehaviour(
        MageRainSpell mageRainSpellPrefab,
        float rainMoveDuration,
        float rainSpawnDuration,
        float rainFillDuration,
        float rainRecoveryDuration)
    {
        this.mageRainSpellPrefab = mageRainSpellPrefab;
        this.rainMoveDuration = rainMoveDuration;
        this.rainSpawnDuration = rainSpawnDuration;
        this.rainFillDuration = rainFillDuration;
        this.rainRecoveryDuration = rainRecoveryDuration;
    }

    public void StartBehaviour(EnemyController enemy, BehaviourExecution execution)
    {
        ResetRuntimeState();
        Vector3 randomPosition = UnityEngine.Random.insideUnitSphere * 7.0f;
        randomPosition.y = 0.0f;
        bool second = enemy.currentPhase > 0;
        if (!isSubBehaviour)
        {
            enemy.animator.Play("Cast");
            attackSequence = Sequence.Create()
                .Chain(MoveMageToPosition(enemy, randomPosition))
                .Group(CastRainSpell())
                .ChainDelay(rainRecoveryDuration)
                .ChainCallback(() => execution.Complete());
        }
        else
        {
            attackSequence = CastRainSpell();
        }
    }
    public void UpdateBehaviour(EnemyController enemy) { }
    public void FixedUpdateBehaviour(EnemyController enemy) { }
    public void StopBehaviour(EnemyController enemy)
    {
        if (attackSequence.isAlive && !isSubBehaviour)
            attackSequence.Stop();
    }

    public void CancelBehaviour(EnemyController enemy)
    {
        ResetRuntimeState();
    }

    public void SetSubBehaviourState(bool state)
    {
        isSubBehaviour = state;
    }

    private Sequence CastRainSpell()
    {
        return Sequence.Create().ChainCallback(() =>
        {
            spell = UnityEngine.Object.Instantiate(
                mageRainSpellPrefab,
                Vector3.zero,
                Quaternion.Euler(90.0f, 0.0f, 0.0f));
            spell.Setup(0.1f, rainSpawnDuration, rainFillDuration);
        });
    }
    private Sequence MoveMageToPosition(EnemyController enemy, Vector3 position)
    {
        bool second = enemy.currentPhase > 0;
        float duration = rainMoveDuration;
        return Sequence.Create()
            .ChainDelay(0.5f)
            .ChainCallback(() =>
            {
                if (second)
                    enemy.afterImage.Trigger(duration);

                MageSFX.instance.PlayMageMove();
            })
            .Group(Tween.Position(enemy.transform, position, duration, Ease.InOutCubic));
    }

    private void ResetRuntimeState()
    {
        if (attackSequence.isAlive)
            attackSequence.Stop();

        if (spell != null)
            spell.Cancel();

        attackSequence = default;
        spell = null;
    }
}
