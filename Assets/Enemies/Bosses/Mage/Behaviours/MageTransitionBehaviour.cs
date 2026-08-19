using System;
using System.Collections.Generic;
using Enemies.Scripts;
using Enemies.Scripts.Behaviours;
using PrimeTween;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

[Serializable]
public sealed class MageTransitionBehaviour : IEnemyBehaviour
{
    private enum TransitionPhase
    {
        Immune,
        Stun,
        Rage
    }

    [OdinSerialize, Required] private CircleDamageZone circleDamageZonePrefab;
    [OdinSerialize, Required] private HollowCircleDamageZone hollowCircleDamageZonePrefab;
    [OdinSerialize, Required] private Damageable pillarPrefab;
    [OdinSerialize, Required] private MageRainBehaviour attackBehaviour;
    [OdinSerialize] private float attackCooldown;
    [OdinSerialize] private float rageAttackCooldown;
    [OdinSerialize, Required] private MageSwipeHorizontalBehaviour rageBehaviour1;
    [OdinSerialize, Required] private MageSwipeVerticalBehaviour rageBehaviour2;

    [NonSerialized] private float lastAttackTimestamp;
    [NonSerialized] private bool isAttackBehaviourActive;
    [NonSerialized] private int stunStartHealth;
    [NonSerialized] private float stunStartTimestamp;
    [NonSerialized] private TransitionPhase currentPhase;
    [NonSerialized] private Transform topPillar;
    [NonSerialized] private Transform botPillar;
    [NonSerialized] private int brokenPillarCount;
    [NonSerialized] private Sequence immuneSequence;
    [NonSerialized] private Sequence stunSequence;
    [NonSerialized] private Sequence rageSequence;
    [NonSerialized] private BehaviourExecution activeExecution;
    [NonSerialized] private List<CircleDamageZone> initialDamageZones = new List<CircleDamageZone>();
    [NonSerialized] private List<HollowCircleDamageZone> hollowCircles = new List<HollowCircleDamageZone>();

    public MageTransitionBehaviour()
    {
    }

    public MageTransitionBehaviour(
        CircleDamageZone circleDamageZonePrefab,
        HollowCircleDamageZone hollowCircleDamageZonePrefab,
        Damageable pillarPrefab,
        MageRainBehaviour attackBehaviour,
        float attackCooldown,
        float rageAttackCooldown,
        MageSwipeHorizontalBehaviour rageBehaviour1,
        MageSwipeVerticalBehaviour rageBehaviour2)
    {
        this.circleDamageZonePrefab = circleDamageZonePrefab;
        this.hollowCircleDamageZonePrefab = hollowCircleDamageZonePrefab;
        this.pillarPrefab = pillarPrefab;
        this.attackBehaviour = attackBehaviour;
        this.attackCooldown = attackCooldown;
        this.rageAttackCooldown = rageAttackCooldown;
        this.rageBehaviour1 = rageBehaviour1;
        this.rageBehaviour2 = rageBehaviour2;
    }

    public void StartBehaviour(EnemyController enemy, BehaviourExecution execution)
    {
        ResetRuntimeState(enemy);
        activeExecution = execution;
        currentPhase = TransitionPhase.Immune;
        enemy.DeactivateHitbox();
        Vector3 upperPillarPosition = new Vector3(7.5f, 0.0f, 7.5f);
        Vector3 lowerPillarPosition = new Vector3(-7.5f, 0.0f, -7.5f);
        topPillar = SpawnPillar(enemy, upperPillarPosition, execution);
        botPillar = SpawnPillar(enemy, lowerPillarPosition, execution);
        immuneSequence = Sequence.Create()
            .Chain(Tween.LocalPosition(enemy.transform, Vector3.zero, 0.5f, Ease.InOutCubic))
            .Chain(Tween.LocalPosition(enemy.Sprite.transform, Vector3.up * 3.0f, 0.5f, Ease.OutBack))
            .ChainCallback(() => enemy.animator.Play("Charge"))
            .ChainCallback(() => SpawnInitialDamageZone(upperPillarPosition))
            .ChainDelay(0.6f)
            .Chain(Tween.PositionY(topPillar, 0.0f, 0.3f, Ease.OutBounce))
            .ChainCallback(() => SpawnHollowCircle(upperPillarPosition))
            .Group(Tween.PunchScale(topPillar, new Vector3(0.5f, -0.5f, 0.0f), 0.15f, 3.0f))
            .ChainCallback(() => topPillar.GetComponent<MagePillarLine>().SetTarget(enemy.transform, new Vector3(7.21f, 3.87f, 8.76f)))
            .ChainDelay(2.0f)
            .ChainCallback(() => SpawnInitialDamageZone(lowerPillarPosition))
            .ChainDelay(0.6f)
            .Chain(Tween.PositionY(botPillar, 0.0f, 0.3f, Ease.OutBounce))
            .ChainCallback(() => SpawnHollowCircle(lowerPillarPosition))
            .Group(Tween.PunchScale(botPillar, new Vector3(0.5f, -0.5f, 0.0f), 0.15f, 3.0f))
            .ChainCallback(() => botPillar.GetComponent<MagePillarLine>().SetTarget(enemy.transform, new Vector3(-7.12f, 3.87f, -5.73f)))
            .ChainCallback(() => StartSecondaryBehaviour(enemy));
    }
    public void UpdateBehaviour(EnemyController enemy)
    {
        bool attackPhase = currentPhase == TransitionPhase.Immune || currentPhase == TransitionPhase.Rage;
        float cooldown = currentPhase == TransitionPhase.Rage ? rageAttackCooldown : attackCooldown;

        if (isAttackBehaviourActive
            && attackPhase
            && Time.time - lastAttackTimestamp >= cooldown)
        {
            attackBehaviour.StartBehaviour(enemy, BehaviourExecution.Uncontrolled);
            lastAttackTimestamp = Time.time;
        }

        if (currentPhase == TransitionPhase.Stun
            && (enemy.damageable.currentHealth <= stunStartHealth - 50
                || Time.time - stunStartTimestamp >= 5.0f))
        {
            StartRageSequence(enemy, activeExecution);
        }
    }

    public void FixedUpdateBehaviour(EnemyController enemy) { }
    public void StopBehaviour(EnemyController enemy) { }

    public void CancelBehaviour(EnemyController enemy)
    {
        ResetRuntimeState(enemy);
    }

    public void SetSubBehaviourState(bool state) { }

    private void SpawnInitialDamageZone(Vector3 position)
    {
        CircleDamageZone zone = UnityEngine.Object.Instantiate(
            circleDamageZonePrefab,
            position,
            Quaternion.Euler(90.0f, 0.0f, 0.0f));
        zone.Setup(0.15f, 0.6f, 0.3f);
        initialDamageZones.Add(zone);
    }

    private Transform SpawnPillar(EnemyController enemy, Vector3 position, BehaviourExecution execution)
    {
        Damageable pillar = UnityEngine.Object.Instantiate(
            pillarPrefab,
            position + Vector3.up * 15.0f,
            Quaternion.identity);
        pillar.OnDie.AddListener(() =>
        {
            if (enemy.IsExecutionActive(execution))
                OnBreakPillar(enemy);
        });
        return pillar.transform;
    }

    private void SpawnHollowCircle(Vector3 position)
    {
        HollowCircleDamageZone circle = UnityEngine.Object.Instantiate(
            hollowCircleDamageZonePrefab,
            position,
            Quaternion.Euler(90.0f, 0.0f, 0.0f));
        circle.Setup();
        hollowCircles.Add(circle);
    }

    private void OnBreakPillar(EnemyController enemy)
    {
        brokenPillarCount++;

        if (brokenPillarCount == 2)
        {
            currentPhase = TransitionPhase.Stun;
            stunStartHealth = enemy.damageable.currentHealth;
            stunStartTimestamp = Time.time;
            RockOrbiter.instance.HideRocks();
            stunSequence = Sequence.Create()
                .Group(Tween.LocalPositionY(enemy.Sprite.transform, 0.0f, 0.15f, Ease.OutBounce))
                .ChainCallback(() => enemy.animator.Play("Stun"))
                .ChainCallback(() => enemy.ActivateHitbox());
        }
    }

    private void StartSecondaryBehaviour(EnemyController enemy)
    {
        isAttackBehaviourActive = true;
        attackBehaviour.SetSubBehaviourState(true);
        attackBehaviour.StartBehaviour(enemy, BehaviourExecution.Uncontrolled);
        lastAttackTimestamp = Time.time;
    }

    private void StartRageSequence(EnemyController enemy, BehaviourExecution execution)
    {
        currentPhase = TransitionPhase.Rage;
        lastAttackTimestamp = Time.time;
        enemy.animator.Play("Charge");
        RockOrbiter.instance.DisplayRocks();
        RockOrbiter.instance.SetRockSpeed(1.0f);
        rageBehaviour1.SetSubBehaviourState(true);
        rageBehaviour2.SetSubBehaviourState(true);

        rageSequence = Sequence.Create()
            .ChainCallback(() => rageBehaviour1.StartBehaviour(enemy, BehaviourExecution.Uncontrolled))
            .ChainDelay(0.1f)
            .ChainCallback(() => rageBehaviour2.StartBehaviour(enemy, BehaviourExecution.Uncontrolled))
            .ChainDelay(3.0f)
            .ChainCallback(() => rageBehaviour1.StartBehaviour(enemy, BehaviourExecution.Uncontrolled))
            .ChainDelay(0.1f)
            .ChainCallback(() => rageBehaviour2.StartBehaviour(enemy, BehaviourExecution.Uncontrolled))
            .ChainDelay(3.0f)
            .ChainCallback(() => rageBehaviour1.StartBehaviour(enemy, BehaviourExecution.Uncontrolled))
            .ChainDelay(0.1f)
            .ChainCallback(() => rageBehaviour2.StartBehaviour(enemy, BehaviourExecution.Uncontrolled))
            .ChainDelay(3.0f)
            .ChainCallback(() => rageBehaviour1.StartBehaviour(enemy, BehaviourExecution.Uncontrolled))
            .ChainDelay(0.1f)
            .ChainCallback(() => rageBehaviour2.StartBehaviour(enemy, BehaviourExecution.Uncontrolled))
            .ChainDelay(3.0f)
            .ChainCallback(() => lastAttackTimestamp = Time.time)
            .ChainDelay(0.5f)
            .ChainCallback(() => ResetAttackBehaviour(enemy))
            .ChainCallback(() =>
            {
                ResetRageBehaviours(enemy);
                execution.Complete();
            });
    }

    private void ResetAttackBehaviour(EnemyController enemy)
    {
        isAttackBehaviourActive = false;
        attackBehaviour.CancelBehaviour(enemy);
        attackBehaviour.SetSubBehaviourState(false);
    }

    private void ResetRageBehaviours(EnemyController enemy)
    {
        rageBehaviour1.CancelBehaviour(enemy);
        rageBehaviour1.SetSubBehaviourState(false);
        rageBehaviour2.CancelBehaviour(enemy);
        rageBehaviour2.SetSubBehaviourState(false);
    }

    private void ResetRuntimeState(EnemyController enemy)
    {
        if (initialDamageZones == null)
            initialDamageZones = new List<CircleDamageZone>();

        if (hollowCircles == null)
            hollowCircles = new List<HollowCircleDamageZone>();

        if (immuneSequence.isAlive)
            immuneSequence.Stop();

        if (stunSequence.isAlive)
            stunSequence.Stop();

        if (rageSequence.isAlive)
            rageSequence.Stop();

        attackBehaviour?.CancelBehaviour(enemy);
        attackBehaviour?.SetSubBehaviourState(false);
        rageBehaviour1?.CancelBehaviour(enemy);
        rageBehaviour1?.SetSubBehaviourState(false);
        rageBehaviour2?.CancelBehaviour(enemy);
        rageBehaviour2?.SetSubBehaviourState(false);
        immuneSequence = default;
        stunSequence = default;
        rageSequence = default;
        activeExecution = null;
        topPillar = null;
        botPillar = null;
        brokenPillarCount = 0;
        stunStartHealth = 0;
        stunStartTimestamp = 0.0f;
        lastAttackTimestamp = 0.0f;
        isAttackBehaviourActive = false;
        initialDamageZones.Clear();
        hollowCircles.Clear();
    }
}
