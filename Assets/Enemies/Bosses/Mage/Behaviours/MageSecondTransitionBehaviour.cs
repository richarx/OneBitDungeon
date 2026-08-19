using System;
using System.Collections.Generic;
using Enemies.Scripts.Behaviours;
using Player.Scripts;
using PrimeTween;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

[Serializable]
public sealed class MageSecondTransitionBehaviour : IEnemyBehaviour
{
    [OdinSerialize, Required] private HollowCircleDamageZone hollowCircleDamageZonePrefab;
    [OdinSerialize] private int maxBounceCount;

    [NonSerialized] private Sequence bounceSequence;
    [NonSerialized] private int bounceCount;
    [NonSerialized] private BehaviourExecution activeExecution;
    [NonSerialized] private List<HollowCircleDamageZone> hollowCircles = new List<HollowCircleDamageZone>();
    public MageSecondTransitionBehaviour()
    {
    }

    public MageSecondTransitionBehaviour(HollowCircleDamageZone hollowCircleDamageZonePrefab, int maxBounceCount)
    {
        this.hollowCircleDamageZonePrefab = hollowCircleDamageZonePrefab;
        this.maxBounceCount = maxBounceCount;
    }

    public void StartBehaviour(EnemyController enemy, BehaviourExecution execution)
    {
        ResetRuntimeState(enemy, false);
        activeExecution = execution;
        bounceSequence = Sequence.Create()
            .Chain(Tween.LocalPosition(enemy.transform, Vector3.zero, 1.0f, Ease.InOutCubic))
            .ChainCallback(() => enemy.DeactivateHitbox())
            .ChainCallback(() => enemy.animator.Play("Charge"))
            .Chain(Tween.LocalPositionY(enemy.Sprite.transform, 5.0f, 1.5f, Ease.InOutCubic))
            .Chain(Tween.ShakeScale(enemy.Sprite.transform, Vector3.up, 0.5f))
            .ChainCallback(() => enemy.animator.Play("Ball"));
    }

    public void UpdateBehaviour(EnemyController enemy)
    {
        if (bounceSequence.isAlive)
            return;

        bounceCount++;

        if (bounceCount > maxBounceCount)
            StopTransition(enemy, activeExecution);
        else
            BounceOnce(enemy, ComputeRandomPosition());
    }

    public void FixedUpdateBehaviour(EnemyController enemy) { }

    public void StopBehaviour(EnemyController enemy)
    {
        ResetRuntimeState(enemy, true);
    }

    public void CancelBehaviour(EnemyController enemy)
    {
        ResetRuntimeState(enemy, true);
    }

    public void SetSubBehaviourState(bool state) { }

    private void BounceOnce(EnemyController enemy, Vector3 position)
    {
        bounceSequence = Sequence.Create()
            .Chain(Tween.LocalPosition(enemy.transform, position, 0.3f, Ease.InOutCubic))
            .Chain(Tween.LocalPositionY(enemy.Sprite.transform, 8.0f, 0.3f, Ease.OutBack))
            .Chain(Tween.LocalPositionY(enemy.Sprite.transform, 0.0f, 0.1f, Ease.OutBack))
            .ChainCallback(() => SpawnHollowCircle(position))
            .Chain(Tween.LocalPositionY(enemy.Sprite.transform, 8.0f, 0.5f, Ease.OutBack))
            .Chain(Tween.LocalPosition(enemy.transform, ComputeRandomPosition(), 0.5f, Ease.InOutCubic));
    }

    private Vector3 ComputeRandomPosition()
    {
        Vector3 playerPosition = PlayerStateMachine.instance.position;
        const float range = 7.5f;
        Vector3 position = new Vector3(
            UnityEngine.Random.Range(-range, range),
            0.0f,
            UnityEngine.Random.Range(-range, range));

        return Vector3.Distance(position, playerPosition) <= 2.0f
            ? playerPosition + (playerPosition * -1.0f).normalized * 3.0f
            : position;
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

    private void StopTransition(EnemyController enemy, BehaviourExecution execution)
    {
        bounceSequence = Sequence.Create()
            .Chain(Tween.LocalPosition(enemy.transform, Vector3.zero, 1.5f, Ease.InOutCubic))
            .Chain(Tween.LocalPositionY(enemy.Sprite.transform, 0.0f, 0.5f, Ease.OutBack))
            .ChainCallback(() => enemy.animator.Play("Blast"))
            .ChainCallback(() => enemy.ActivateHitbox())
            .ChainCallback(() => execution.Complete());
    }

    private void ResetRuntimeState(EnemyController enemy, bool activateHitbox)
    {
        if (hollowCircles == null)
            hollowCircles = new List<HollowCircleDamageZone>();

        if (activateHitbox)
            enemy.ActivateHitbox();

        if (bounceSequence.isAlive)
            bounceSequence.Stop();

        bounceSequence = default;
        bounceCount = 0;
        activeExecution = null;
        hollowCircles.Clear();
        enemy.Sprite.transform.localPosition = Vector3.zero;
    }
}
