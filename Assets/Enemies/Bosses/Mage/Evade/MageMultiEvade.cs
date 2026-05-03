using System.Collections;
using System.Collections.Generic;
using Enemies.Scripts.Behaviours;
using Player.Scripts;
using PrimeTween;
using Tools_and_Scripts;
using UnityEngine;

public class MageMultiEvade : MonoBehaviour, IEnemyBehaviour
{
    [SerializeField] private CircleDamageZone circleDamageZonePrefab;
    [SerializeField] private HollowCircleDamageZone hollowCircleDamageZonePrefab;

    private CircleDamageZone circle;
    private Sequence currentSequence;
    private Sequence moveRockSequence;
    private Sequence detonateRockSequence;

    private List<SpinRock> rocks;

    public void StartBehaviour(EnemyController enemy)
    {
        Debug.Log("Mage MULTI EVADE");

        Vector3 currentPosition = enemy.transform.position;
        Vector3 playerPosition = PlayerStateMachine.instance.position;
        Vector3 targetPosition = playerPosition + (currentPosition - playerPosition).normalized * 0.5f;

        Vector3 evadePosition = targetPosition.magnitude <= 0.01f ? Vector3.forward * 7.0f : (targetPosition * -1.0f).normalized * 7.0f;
        Vector3 evadePosition_2 = ComputeRandomPosition();
        Vector3 evadePosition_3 = ComputeRandomPosition();

        bool isSecondPhase = enemy.currentPhase > 0;

        float moveDuration = isSecondPhase ? 0.3f : 0.5f;

        rocks = new List<SpinRock>();
        for (int i = 0; i < 9; i++)
            rocks.Add(RockOrbiter.instance.GetRandomRock());

        currentSequence = Sequence.Create()
            .ChainCallback(() => SpawnDamageZone(targetPosition))
            .ChainCallback(() =>
            {
                if (isSecondPhase)
                    enemy.afterImage.Trigger(moveDuration);
                MageSFX.instance.PlayMageMove();
            })
            .ChainCallback(() => MoveRocksToStartingPosition(targetPosition, 0))
            .Group(Tween.Position(enemy.transform, targetPosition, moveDuration, Ease.InOutCubic))
            .ChainCallback(() => enemy.animator.Play("Blast"))
            .ChainCallback(() => DetonateRocks(moveDuration, 0, false, targetPosition))
            .Chain(Tween.ScaleX(enemy.transform, 0.0f, 0.3f, Ease.InBack))
            .ChainCallback(() => enemy.transform.position = evadePosition)
            .Chain(Tween.ScaleX(enemy.transform, 1.0f, 0.3f, Ease.OutBack))

            .ChainCallback(() => SpawnDamageZone(evadePosition))
            .ChainCallback(() =>
            {
                if (isSecondPhase)
                    enemy.afterImage.Trigger(moveDuration);
                MageSFX.instance.PlayMageMove();
            })
            .ChainCallback(() => MoveRocksToStartingPosition(evadePosition, 3))
            .ChainCallback(() => enemy.animator.Play("Blast"))
            .ChainCallback(() => DetonateRocks(moveDuration, 3, false, evadePosition))
            .Chain(Tween.ScaleX(enemy.transform, 0.0f, 0.3f, Ease.InBack))
            .ChainCallback(() => enemy.transform.position = evadePosition_2)
            .Chain(Tween.ScaleX(enemy.transform, 1.0f, 0.3f, Ease.OutBack))

            .ChainCallback(() => SpawnDamageZone(evadePosition_2))
            .ChainCallback(() =>
            {
                if (isSecondPhase)
                    enemy.afterImage.Trigger(moveDuration);
                MageSFX.instance.PlayMageMove();
            })
            .ChainCallback(() => MoveRocksToStartingPosition(evadePosition_2, 6))
            .ChainCallback(() => enemy.animator.Play("Blast"))
            .ChainCallback(() => DetonateRocks(moveDuration, 6, true, evadePosition_2))
            .Chain(Tween.ScaleX(enemy.transform, 0.0f, 0.3f, Ease.InBack))
            .ChainCallback(() => enemy.transform.position = evadePosition_3)
            .Chain(Tween.ScaleX(enemy.transform, 1.0f, 0.3f, Ease.OutBack))

            .ChainDelay(isSecondPhase ? 0.5f : 0.75f)
            .ChainCallback(() => enemy.SelectNewBehaviour());
    }

    private Vector3 ComputeRandomPosition()
    {
        float range = 7.5f;
        float x = Random.Range(-range, range);
        float z = Random.Range(-range, range);
        return new Vector3(x, 0.0f, z);
    }

    private void SpawnDamageZone(Vector3 position)
    {
        circle = Instantiate(circleDamageZonePrefab, position, Quaternion.Euler(new Vector3(90.0f, 0.0f, 0.0f)));
        circle.Setup();
    }

    private void SpawnHollowCircle(Vector3 position)
    {
        HollowCircleDamageZone hollowCircle = Instantiate(hollowCircleDamageZonePrefab, position, Quaternion.Euler(new Vector3(90.0f, 0.0f, 0.0f)));
        hollowCircle.Setup();
    }

    private void MoveRocksToStartingPosition(Vector3 targetPosition, int rockIndex)
    {
        rocks[rockIndex].SetState(SpinRock.RockState.Locked);
        rocks[rockIndex + 1].SetState(SpinRock.RockState.Locked);
        rocks[rockIndex + 2].SetState(SpinRock.RockState.Locked);

        float z = Mathf.Clamp(targetPosition.z, -10.0f, 0.0f);
        float height = 8.0f + 4.0f * Tools.NormalizeValue(z, -10.0f, 0.0f);

        Vector3 rockPosition = targetPosition + Vector3.up * height;
        float angle = Random.Range(0.0f, 360.0f);
        float distanceFromCenter = 1.5f;

        moveRockSequence = Sequence.Create()
            .Group(Tween.LocalPosition(rocks[rockIndex].transform, rockPosition + Vector2.right.AddAngleToDirection(angle).ToVector3() * distanceFromCenter, 0.5f, Ease.InOutBack))
            .Group(Tween.LocalPosition(rocks[rockIndex + 1].transform, rockPosition + Vector2.right.AddAngleToDirection(angle + 120.0f).ToVector3() * distanceFromCenter, 0.5f, Ease.InOutBack))
            .Group(Tween.LocalPosition(rocks[rockIndex + 2].transform, rockPosition + Vector2.right.AddAngleToDirection(angle + 240.0f).ToVector3() * distanceFromCenter, 0.5f, Ease.InOutBack))
            ;

        MageSFX.instance.PlayRockMove();
    }

    private void DetonateRocks(float moveDuration, int rockIndex, bool isSpawningHollowCircle, Vector3 position)
    {
        detonateRockSequence = Sequence.Create()
            .ChainDelay(1.0f - moveDuration - 0.1f)
            .Chain(Tween.LocalPositionY(rocks[rockIndex].transform, 0.0f, 0.3f, Ease.InOutBack))
            .Group(Tween.LocalPositionY(rocks[rockIndex + 1].transform, 0.0f, 0.3f, Ease.InOutBack))
            .Group(Tween.LocalPositionY(rocks[rockIndex + 2].transform, 0.0f, 0.3f, Ease.InOutBack))
            .Chain(Tween.PunchScale(rocks[rockIndex].transform, new Vector3(1.3f, 0.7f, 1.0f), 0.1f, 3.0f))
            .Group(Tween.PunchScale(rocks[rockIndex + 1].transform, new Vector3(1.3f, 0.7f, 1.0f), 0.1f, 3.0f))
            .Group(Tween.PunchScale(rocks[rockIndex + 2].transform, new Vector3(1.3f, 0.7f, 1.0f), 0.1f, 3.0f))
            .ChainCallback(() => SpawnDebris(rockIndex))
            .ChainCallback(() =>
            {
                if (isSpawningHollowCircle)
                    SpawnHollowCircle(position);
            })
            .ChainCallback(() => ReturnRocksToOrbiter(rockIndex));
    }

    private void SpawnDebris(int rockIndex)
    {
        if (rockIndex + 2 < rocks.Count)
        {
            SpawnSingleDebris(rocks[rockIndex]);
            SpawnSingleDebris(rocks[rockIndex + 1]);
            SpawnSingleDebris(rocks[rockIndex + 2]);
        }
    }

    private void SpawnSingleDebris(SpinRock rock)
    {
        if (rock != null)
            RockOrbiter.instance.SpawnDebris(rock.transform.position);
    }

    private void ReturnRocksToOrbiter(int rockIndex)
    {
        if (rockIndex + 2 < rocks.Count)
        {
            RockOrbiter.instance.AddBackRock(rocks[rockIndex]);
            RockOrbiter.instance.AddBackRock(rocks[rockIndex + 1]);
            RockOrbiter.instance.AddBackRock(rocks[rockIndex + 2]);
        }
        else
            Debug.Log($"Zuzu : Failed to return rocks : {rockIndex} / {rocks.Count}");
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
        if (currentSequence.isAlive)
            currentSequence.Stop();

        if (moveRockSequence.isAlive)
            moveRockSequence.Stop();

        if (detonateRockSequence.isAlive)
            detonateRockSequence.Stop();

        if (circle != null)
            circle.Cancel();

        SpawnDebris(0);
        SpawnDebris(3);
        SpawnDebris(6);
        ReturnRocksToOrbiter(0);
        ReturnRocksToOrbiter(3);
        ReturnRocksToOrbiter(6);
    }

    public bool isSubBehaviour;
    public void SetSubBehaviourState(bool state)
    {
        isSubBehaviour = state;
    }
}
