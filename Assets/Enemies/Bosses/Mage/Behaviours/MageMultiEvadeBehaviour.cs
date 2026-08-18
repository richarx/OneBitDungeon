using System;
using System.Collections.Generic;
using Enemies.Scripts.Behaviours;
using Player.Scripts;
using PrimeTween;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

[Serializable]
public sealed class MageMultiEvadeBehaviour : IEnemyBehaviour
{
    [OdinSerialize] private float radius;
    [OdinSerialize, Required] private MageEvadeSpell mageEvadeSpellPrefab;
    [OdinSerialize, Required] private HollowCircleDamageZone hollowCircleDamageZonePrefab;
    [OdinSerialize, Required] private MageData mageData;

    [NonSerialized] private Sequence attackSequence;
    [NonSerialized] private HollowCircleDamageZone hollowCircle;
    [NonSerialized] private List<MageEvadeSpell> spells = new List<MageEvadeSpell>();

    public MageMultiEvadeBehaviour()
    {
    }

    public MageMultiEvadeBehaviour(
        float radius,
        MageEvadeSpell mageEvadeSpellPrefab,
        HollowCircleDamageZone hollowCircleDamageZonePrefab,
        MageData mageData)
    {
        this.radius = radius;
        this.mageEvadeSpellPrefab = mageEvadeSpellPrefab;
        this.hollowCircleDamageZonePrefab = hollowCircleDamageZonePrefab;
        this.mageData = mageData;
    }

    public void StartBehaviour(EnemyController enemy, BehaviourExecution execution)
    {
        ResetRuntimeState();
        Debug.Log("Mage MULTI EVADE");

        Vector3 currentPosition = enemy.transform.position;
        Vector3 playerPosition = PlayerStateMachine.instance.position;
        Vector3 target = playerPosition + (currentPosition - playerPosition).normalized * 0.5f;
        Vector3 evade = target.magnitude <= 0.01f
            ? Vector3.forward * 7.0f
            : (target * -1.0f).normalized * 7.0f;
        Vector3 evade2 = ComputeRandomPosition();
        Vector3 evade3 = ComputeRandomPosition();

        attackSequence = Sequence.Create()
            .ChainCallback(() => SpawnDamageZone(target))
            .Chain(MoveMageToPosition(enemy, target))
            .Chain(TeleportMageToPosition(enemy, evade))
            .ChainCallback(() => SpawnDamageZone(evade))
            .Chain(MoveMageToPosition(enemy, evade))
            .Chain(TeleportMageToPosition(enemy, evade2))
            .ChainCallback(() => SpawnDamageZone(evade2, () => SpawnHollowCircle(evade2)))
            .Chain(MoveMageToPosition(enemy, evade2))
            .Chain(TeleportMageToPosition(enemy, evade3))
            .ChainDelay(mageData.multiEvadeRecoveryDuration)
            .ChainCallback(() => execution.Complete());
    }

    public void UpdateBehaviour(EnemyController enemy) { }
    public void FixedUpdateBehaviour(EnemyController enemy) { }
    public void StopBehaviour(EnemyController enemy) { }

    public void CancelBehaviour(EnemyController enemy)
    {
        ResetRuntimeState();
        enemy.transform.localScale = Vector3.one;
    }

    public void SetSubBehaviourState(bool state) { }

    private Sequence MoveMageToPosition(EnemyController enemy, Vector3 position)
    {
        return Sequence.Create()
            .ChainCallback(() =>
            {
                enemy.afterImage.Trigger(mageData.multiEvadeSpawnDuration);
                MageSFX.instance.PlayMageMove();
            })
            .Group(Tween.Position(enemy.transform, position, mageData.multiEvadeSpawnDuration, Ease.InOutCubic));
    }

    private Sequence TeleportMageToPosition(EnemyController enemy, Vector3 position)
    {
        return Sequence.Create()
            .ChainCallback(() => enemy.animator.Play("Blast"))
            .Chain(Tween.ScaleX(enemy.transform, 0.0f, 0.3f, Ease.InBack))
            .ChainCallback(() => enemy.transform.position = position)
            .Chain(Tween.ScaleX(enemy.transform, 1.0f, 0.3f, Ease.OutBack));
    }

    private static Vector3 ComputeRandomPosition()
    {
        const float range = 7.5f;
        return new Vector3(
            UnityEngine.Random.Range(-range, range),
            0.0f,
            UnityEngine.Random.Range(-range, range));
    }

    private void SpawnDamageZone(Vector3 position, Action onShootCallback = null)
    {
        MageEvadeSpell spell = UnityEngine.Object.Instantiate(
            mageEvadeSpellPrefab,
            position,
            Quaternion.Euler(90.0f, 0.0f, 0.0f));
        spell.Setup(radius, mageData.multiEvadeSpawnDuration, mageData.multiEvadeFillDuration, onShootCallback);
        spells.Add(spell);
    }

    private void SpawnHollowCircle(Vector3 position)
    {
        hollowCircle = UnityEngine.Object.Instantiate(
            hollowCircleDamageZonePrefab,
            position,
            Quaternion.Euler(90.0f, 0.0f, 0.0f));
        hollowCircle.Setup();
    }

    private void ResetRuntimeState()
    {
        if (spells == null)
            spells = new List<MageEvadeSpell>();

        if (attackSequence.isAlive)
            attackSequence.Stop();

        foreach (MageEvadeSpell spell in spells)
        {
            if (spell != null)
                spell.Cancel();
        }

        if (hollowCircle != null)
            hollowCircle.Cancel();

        attackSequence = default;
        hollowCircle = null;
        spells.Clear();
    }
}
