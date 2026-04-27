using System;
using Enemies.Scripts;
using Enemies.Scripts.Behaviours;
using PrimeTween;
using UnityEngine;

public class MageTransition : MonoBehaviour, IEnemyBehaviour
{
    private enum TransitionPhase
    {
        Immune,
        Stun,
        Rage
    }

    [SerializeField] private CircleDamageZone circleDamageZonePrefab;
    [SerializeField] private HollowCircleDamageZone hollowCircleDamageZonePrefab;
    [SerializeField] private Damageable pillarPrefab;

    [Space]
    [SerializeField] private GameObject attackBehaviourObject;
    [SerializeField] private float attackCooldown;
    [SerializeField] private float rageAttackCooldown;

    [Space]
    [SerializeField] private GameObject rageBehaviourObject_1;
    [SerializeField] private GameObject rageBehaviourObject_2;

    private bool isSubBehaviour;

    private IEnemyBehaviour attackBehaviour;
    private float lastAttackTimestamp = 0.0f;

    private int stunStartHealth;
    private float stunStartTimestamp;

    private TransitionPhase currentPhase;

    private Transform topPillar;
    private Transform botPillar;

    private int brokenPillarCount;

    public void StartBehaviour(EnemyController enemy)
    {
        currentPhase = TransitionPhase.Immune;
        enemy.DeactivateHitbox();

        Vector3 upperPillarPosition = new Vector3(7.5f, 0.0f, 7.5f);
        topPillar = SpawnPillar(enemy, upperPillarPosition);

        Vector3 lowerPillarPosition = new Vector3(-7.5f, 0.0f, -7.5f);
        botPillar = SpawnPillar(enemy, lowerPillarPosition);

        brokenPillarCount = 0;

        Sequence.Create()
            .Chain(Tween.LocalPosition(enemy.transform, Vector3.zero, 0.5f, Ease.InOutCubic))
            .Chain(Tween.LocalPosition(enemy.sprite.transform, Vector3.up * 3.0f, 0.5f, Ease.OutBack))
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

    private void SpawnInitialDamageZone(Vector3 position)
    {
        CircleDamageZone circleDamageZone = Instantiate(circleDamageZonePrefab, position, Quaternion.Euler(new Vector3(90.0f, 0.0f, 0.0f)));
        circleDamageZone.Setup();
    }

    private Transform SpawnPillar(EnemyController enemy, Vector3 position)
    {
        position += Vector3.up * 15.0f;
        Damageable damageable = Instantiate(pillarPrefab, position, Quaternion.identity);
        damageable.OnDie.AddListener(() => OnBreakPillar(enemy));
        return damageable.transform;
    }

    private void SpawnHollowCircle(Vector3 position)
    {
        HollowCircleDamageZone hollowCircle = Instantiate(hollowCircleDamageZonePrefab, position, Quaternion.Euler(new Vector3(90.0f, 0.0f, 0.0f)));
        hollowCircle.Setup();
    }

    private void OnBreakPillar(EnemyController enemy)
    {
        brokenPillarCount += 1;

        if (brokenPillarCount == 2)
        {
            currentPhase = TransitionPhase.Stun;
            stunStartHealth = enemy.damageable.currentHealth;
            stunStartTimestamp = Time.time;

            Sequence.Create()
                .Group(Tween.LocalPositionY(enemy.sprite.transform, 0.0f, 0.15f, Ease.OutBounce))
                .ChainCallback(() => enemy.animator.Play("Stun"))
                .ChainCallback(() => enemy.ActivateHitbox());
        }
    }

    private void StartSecondaryBehaviour(EnemyController enemy)
    {
        attackBehaviour = attackBehaviourObject.GetComponent<IEnemyBehaviour>();
        attackBehaviour.SetSubBehaviourState(true);
        attackBehaviour.StartBehaviour(enemy);
        lastAttackTimestamp = Time.time;
    }

    public void UpdateBehaviour(EnemyController enemy)
    {
        bool isAttackPhase = currentPhase == TransitionPhase.Immune || currentPhase == TransitionPhase.Rage;
        bool isAttackTime = Time.time - lastAttackTimestamp >= (currentPhase == TransitionPhase.Rage ? rageAttackCooldown : attackCooldown);

        if (isAttackPhase && attackBehaviour != null && isAttackTime)
        {
            attackBehaviour.StartBehaviour(enemy);
            lastAttackTimestamp = Time.time;
        }

        if (currentPhase == TransitionPhase.Stun && (enemy.damageable.currentHealth <= stunStartHealth - 5 || Time.time - stunStartTimestamp >= 5.0f))
            StartRageSequence(enemy);
    }

    private void StartRageSequence(EnemyController enemy)
    {
        currentPhase = TransitionPhase.Rage;
        lastAttackTimestamp = Time.time;

        enemy.animator.Play("Charge");

        IEnemyBehaviour rageBehaviour_1 = rageBehaviourObject_1.GetComponent<IEnemyBehaviour>();
        rageBehaviour_1.SetSubBehaviourState(true);

        IEnemyBehaviour rageBehaviour_2 = rageBehaviourObject_2.GetComponent<IEnemyBehaviour>();
        rageBehaviour_2.SetSubBehaviourState(true);

        Sequence.Create()
            .ChainCallback(() => rageBehaviour_1.StartBehaviour(enemy))
            .ChainDelay(0.1f)
            .ChainCallback(() => rageBehaviour_2.StartBehaviour(enemy))
            .ChainDelay(1.0f)

            .ChainCallback(() => rageBehaviour_1.StartBehaviour(enemy))
            .ChainDelay(0.1f)
            .ChainCallback(() => rageBehaviour_2.StartBehaviour(enemy))
            .ChainDelay(1.0f)

            .ChainCallback(() => rageBehaviour_1.StartBehaviour(enemy))
            .ChainDelay(0.1f)
            .ChainCallback(() => rageBehaviour_2.StartBehaviour(enemy))
            .ChainDelay(1.0f)

            .ChainCallback(() => rageBehaviour_1.StartBehaviour(enemy))
            .ChainDelay(0.1f)
            .ChainCallback(() => rageBehaviour_2.StartBehaviour(enemy))
            .ChainDelay(1.0f)

            .ChainCallback(() => rageBehaviour_1.StartBehaviour(enemy))
            .ChainDelay(0.1f)
            .ChainCallback(() => rageBehaviour_2.StartBehaviour(enemy))
            .ChainDelay(1.0f)

            .ChainCallback(() => rageBehaviour_1.StartBehaviour(enemy))
            .ChainDelay(0.1f)
            .ChainCallback(() => rageBehaviour_2.StartBehaviour(enemy))
            .ChainDelay(1.0f)

            .ChainCallback(() => rageBehaviour_1.StartBehaviour(enemy))
            .ChainDelay(0.1f)
            .ChainCallback(() => rageBehaviour_2.StartBehaviour(enemy))
            .ChainDelay(1.0f)

            .ChainCallback(() => rageBehaviour_1.StartBehaviour(enemy))
            .ChainDelay(0.1f)
            .ChainCallback(() => rageBehaviour_2.StartBehaviour(enemy))
            .ChainDelay(1.0f)

            .ChainCallback(() => rageBehaviour_1.StartBehaviour(enemy))
            .ChainDelay(0.1f)
            .ChainCallback(() => rageBehaviour_2.StartBehaviour(enemy))
            .ChainDelay(1.0f)

            .ChainCallback(() => lastAttackTimestamp = Time.time)
            .ChainDelay(1.0f)
            .ChainCallback(() =>
            {
                attackBehaviour.StopBehaviour(enemy);
                attackBehaviour.SetSubBehaviourState(false);
                rageBehaviour_1.SetSubBehaviourState(false);
                rageBehaviour_2.SetSubBehaviourState(false);
                enemy.SelectNewBehaviour();
            });
    }

    public void SetSubBehaviourState(bool state)
    {
        isSubBehaviour = state;
    }

    public void FixedUpdateBehaviour(EnemyController enemy)
    {
    }

    public void StopBehaviour(EnemyController enemy)
    {
    }
}
