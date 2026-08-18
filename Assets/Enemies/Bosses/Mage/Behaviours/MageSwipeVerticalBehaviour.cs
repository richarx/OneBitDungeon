using System;
using System.Collections.Generic;
using Enemies.Scripts.Behaviours;
using PrimeTween;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

[Serializable]
public sealed class MageSwipeVerticalBehaviour : IEnemyBehaviour
{
    [OdinSerialize, Required] private MageSwipeSpell mageSwipeSpellPrefab;
    [OdinSerialize, Required] private MageData mageData;

    [NonSerialized] private bool isSubBehaviour;
    [NonSerialized] private Sequence attackSequence;
    [NonSerialized] private List<MageSwipeSpell> spells = new List<MageSwipeSpell>();
    [NonSerialized] private CloseDodgeSession closeDodgeSession;
    [NonSerialized] private bool ownsCloseDodgeSession;

    public MageSwipeVerticalBehaviour()
    {
    }

    public MageSwipeVerticalBehaviour(MageSwipeSpell mageSwipeSpellPrefab, MageData mageData)
    {
        this.mageSwipeSpellPrefab = mageSwipeSpellPrefab;
        this.mageData = mageData;
    }

    public void SetCloseDodgeSession(CloseDodgeSession session)
    {
        closeDodgeSession = session;
        ownsCloseDodgeSession = false;
    }

    public void StartBehaviour(EnemyController enemy, BehaviourExecution execution)
    {
        CloseDodgeSession sharedSession = isSubBehaviour ? closeDodgeSession : null;
        ResetRuntimeState();
        if (isSubBehaviour)
            closeDodgeSession = sharedSession;
        else
        {
            closeDodgeSession = new CloseDodgeSession(5);
            ownsCloseDodgeSession = true;
        }

        Debug.Log("Mage SWIPE VERTICAL");
        bool isSecondPhase = enemy.currentPhase > 0;
        Vector3 randomPosition = UnityEngine.Random.insideUnitSphere * 7.0f;
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
                .ChainCallback(() => execution.Complete());
        }
        else
        {
            attackSequence = CreateSubSequence();
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

    private Sequence CreateSubSequence()
    {
        return Sequence.Create()
            .Group(CastSwipeSpell(new Vector3(8.92f, 0.0f, 10.0f), Vector2.down, 0.0f))
            .Group(CastSwipeSpell(new Vector3(4.42f, 0.0f, -10.0f), Vector2.up, 0.05f))
            .Group(CastSwipeSpell(new Vector3(0.0f, 0.0f, 10.0f), Vector2.down, 0.1f))
            .Group(CastSwipeSpell(new Vector3(-4.58f, 0.0f, -10.0f), Vector2.up, 0.15f))
            .Group(CastSwipeSpell(new Vector3(-9.11f, 0.0f, 10.0f), Vector2.down, 0.2f));
    }

    private Sequence CastSwipeSpell(Vector3 position, Vector2 direction, float delay)
    {
        return Sequence.Create().ChainDelay(delay).ChainCallback(() =>
        {
            MageSwipeSpell spell = UnityEngine.Object.Instantiate(mageSwipeSpellPrefab, position, Quaternion.Euler(90.0f, 0.0f, 0.0f));
            spell.Setup(direction, mageData.swipeSpawnDuration, mageData.swipeFillDuration, closeDodgeSession);
            spells.Add(spell);
        });
    }

    private Sequence MoveMageToPosition(EnemyController enemy, Vector3 position)
    {
        bool isSecondPhase = enemy.currentPhase > 0;
        float duration = isSecondPhase ? mageData.swipeMoveDuration_p2 : mageData.swipeMoveDuration;
        return Sequence.Create().ChainDelay(0.5f).ChainCallback(() =>
        {
            if (isSecondPhase)
                enemy.afterImage.Trigger(duration);
            MageSFX.instance.PlayMageMove();
        }).Group(Tween.Position(enemy.transform, position, duration, Ease.InOutCubic));
    }

    private void ResetRuntimeState()
    {
        if (spells == null)
            spells = new List<MageSwipeSpell>();

        if (attackSequence.isAlive)
            attackSequence.Stop();
        foreach (MageSwipeSpell spell in spells)
        {
            if (spell != null)
                spell.Cancel();
        }

        if (ownsCloseDodgeSession)
            closeDodgeSession?.Cancel();

        attackSequence = default;
        spells.Clear();
        closeDodgeSession = null;
        ownsCloseDodgeSession = false;
    }
}
