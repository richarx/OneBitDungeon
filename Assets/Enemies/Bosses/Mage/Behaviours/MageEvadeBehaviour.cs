using System;
using Enemies.Scripts.Behaviours;
using Player.Scripts;
using PrimeTween;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

[Serializable]
public sealed class MageEvadeBehaviour : IEnemyBehaviour
{
    [OdinSerialize] private float radius;
    [OdinSerialize, Required] private MageEvadeSpell mageEvadeSpellPrefab;
    [OdinSerialize, Required] private MageData mageData;

    [NonSerialized] private Sequence attackSequence;
    [NonSerialized] private MageEvadeSpell spell;

    public MageEvadeBehaviour()
    {
    }

    public MageEvadeBehaviour(float radius, MageEvadeSpell mageEvadeSpellPrefab, MageData mageData)
    {
        this.radius = radius;
        this.mageEvadeSpellPrefab = mageEvadeSpellPrefab;
        this.mageData = mageData;
    }

    public void StartBehaviour(EnemyController enemy, BehaviourExecution execution)
    {
        ResetRuntimeState();
        Debug.Log("Mage EVADE");
        bool isSecondPhase = enemy.currentPhase > 0;
        Vector3 currentPosition = enemy.transform.position;
        Vector3 playerPosition = PlayerStateMachine.instance.position;
        Vector3 target = playerPosition + (currentPosition - playerPosition).normalized * 0.5f;
        Vector3 evade = target.magnitude <= 0.01f ? Vector3.forward * 7.0f : (target * -1.0f).normalized * 7.0f;
        attackSequence = Sequence.Create()
            .ChainCallback(() => SpawnDamageZone(target))
            .Chain(MoveMageToPosition(enemy, target))
            .Chain(TeleportMageToPosition(enemy, evade))
            .ChainDelay(isSecondPhase ? mageData.evadeRecoveryDuration_p2 : mageData.evadeRecoveryDuration)
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
        enemy.transform.localScale = Vector3.one;
    }

    public void SetSubBehaviourState(bool state) { }
    private Sequence MoveMageToPosition(EnemyController enemy, Vector3 position)
    {
        bool isSecondPhase = enemy.currentPhase > 0;
        return Sequence.Create()
            .ChainCallback(() =>
            {
                if (isSecondPhase)
                    enemy.afterImage.Trigger(mageData.evadeSpawnDuration);

                MageSFX.instance.PlayMageMove();
            })
            .Group(Tween.Position(enemy.transform, position, mageData.evadeSpawnDuration, Ease.InOutCubic));
    }
    private Sequence TeleportMageToPosition(EnemyController enemy, Vector3 position)
    {
        return Sequence.Create()
            .ChainCallback(() => enemy.animator.Play("Blast"))
            .Chain(Tween.ScaleX(enemy.transform, 0.0f, 0.3f, Ease.InBack))
            .ChainCallback(() => enemy.transform.position = position)
            .Chain(Tween.ScaleX(enemy.transform, 1.0f, 0.3f, Ease.OutBack));
    }
    private void SpawnDamageZone(Vector3 position)
    {
        spell = UnityEngine.Object.Instantiate(mageEvadeSpellPrefab, position, Quaternion.Euler(90.0f, 0.0f, 0.0f));
        spell.Setup(radius, mageData.evadeSpawnDuration, mageData.evadeFillDuration, null);
    }
    private void ResetRuntimeState()
    {
        if (attackSequence.isAlive) attackSequence.Stop();
        if (spell != null) spell.Cancel();
        attackSequence = default;
        spell = null;
    }
}
