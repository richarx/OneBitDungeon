using Enemies.Scripts.Behaviours;
using Player.Scripts;
using PrimeTween;
using Tools_and_Scripts;
using UnityEngine;
using UnityEngine.UIElements;

public class MageRain : MonoBehaviour, IEnemyBehaviour
{
    [SerializeField] private ThreeCirclesDamageZone threeCirclesDamageZone;

    private bool isSubBehaviour;

    private ThreeCirclesDamageZone circles;
    private Sequence attackSequence;
    private Sequence moveSequence;

    private SpinRock rock_1;
    private SpinRock rock_2;
    private SpinRock rock_3;

    public void StartBehaviour(EnemyController enemy)
    {
        Vector3 randomPosition = Random.insideUnitSphere * 7.0f;
        randomPosition.y = 0.0f;

        if (!isSubBehaviour)
        {
            float moveDuration = enemy.isSecondPhase ? 0.5f : 1.0f;

            moveSequence = Sequence.Create()
                .ChainCallback(() => enemy.animator.Play("Cast"))
                .ChainDelay(0.5f)
                .ChainCallback(() =>
                {
                    if (enemy.isSecondPhase)
                        enemy.afterImage.Trigger(moveDuration);
                })
                .Chain(Tween.Position(enemy.transform, randomPosition, moveDuration, Ease.InOutCubic));
        }

        SpawnDamageZone();

        rock_1 = RockOrbiter.instance.GetRandomRock();
        rock_2 = RockOrbiter.instance.GetRandomRock();
        rock_3 = RockOrbiter.instance.GetRandomRock();

        attackSequence = Sequence.Create()
            .ChainCallback(() =>
            {
                lastKnownPosition = PlayerStateMachine.instance.position;
                SetupCircle(0, lastKnownPosition.ToVector2());

                rock_1.SetLockState(true);
            })
            .ChainCallback(() => MoveRockToStartingPosition(rock_1.transform))
            .ChainDelay(0.5f)
            .ChainCallback(() =>
            {
                lastKnownPosition = PlayerStateMachine.instance.position;
                SetupCircle(1, lastKnownPosition.ToVector2());

                rock_2.SetLockState(true);
            })
            .ChainCallback(() => MoveRockToStartingPosition(rock_2.transform))
            .ChainDelay(0.5f)
            .ChainCallback(() =>
            {
                lastKnownPosition = PlayerStateMachine.instance.position;
                SetupCircle(2, lastKnownPosition.ToVector2());

                rock_3.SetLockState(true);
            })
            .ChainCallback(() => MoveRockToStartingPosition(rock_3.transform))
            .ChainDelay(0.5f)
            .ChainCallback(() => circles.Detonate())
            .ChainCallback(() => DetonateRocks())
            .ChainDelay(enemy.isSecondPhase ? 0.0f : 0.5f)
            .ChainCallback(() =>
            {
                if (!isSubBehaviour)
                    enemy.SelectNewBehaviour();
            });
    }

    private void MoveRockToStartingPosition(Transform rock)
    {
        Sequence.Create()
            .Chain(Tween.LocalPosition(rock, GetLastKnownPosition(), 0.5f, Ease.OutBack));
    }

    private void DetonateRocks()
    {
        Sequence.Create()
            .Group(Tween.LocalPositionY(rock_1.transform, 0.0f, 0.3f, Ease.InOutBack))
            .Group(Tween.LocalPositionY(rock_2.transform, 0.0f, 0.3f, Ease.InOutBack))
            .Group(Tween.LocalPositionY(rock_3.transform, 0.0f, 0.3f, Ease.InOutBack))
            .Chain(Tween.PunchScale(rock_1.transform, new Vector3(1.3f, 0.7f, 1.0f), 0.1f, 3.0f))
            .Group(Tween.PunchScale(rock_2.transform, new Vector3(1.3f, 0.7f, 1.0f), 0.1f, 3.0f))
            .Group(Tween.PunchScale(rock_3.transform, new Vector3(1.3f, 0.7f, 1.0f), 0.1f, 3.0f))
            .ChainCallback(() => SpawnDebris())
            .ChainCallback(() => ReturnRocksToOrbiter());
    }

    private void SpawnDebris()
    {
        RockOrbiter.instance.SpawnDebris(rock_1.transform.position);
        RockOrbiter.instance.SpawnDebris(rock_2.transform.position);
        RockOrbiter.instance.SpawnDebris(rock_3.transform.position);
    }

    Vector3 lastKnownPosition = Vector3.zero;
    private Vector3 GetLastKnownPosition()
    {
        return lastKnownPosition + Vector3.up * 8.0f;
    }

    private void ReturnRocksToOrbiter()
    {
        if (rock_1 != null)
        {
            RockOrbiter.instance.AddBackRock(rock_1);
            rock_1 = null;
        }

        if (rock_2 != null)
        {
            RockOrbiter.instance.AddBackRock(rock_2);
            rock_2 = null;
        }

        if (rock_3 != null)
        {
            RockOrbiter.instance.AddBackRock(rock_3);
            rock_3 = null;
        }
    }

    private void SpawnDamageZone()
    {
        circles = Instantiate(threeCirclesDamageZone, Vector3.zero, Quaternion.Euler(new Vector3(90.0f, 0.0f, 0.0f)));
    }

    private void SetupCircle(int index, Vector2 position)
    {
        circles.Setup(index);
        circles.MoveCircle(index, position);
    }

    public void UpdateBehaviour(EnemyController enemy)
    {

    }

    public void FixedUpdateBehaviour(EnemyController enemy)
    {
    }

    public void StopBehaviour(EnemyController enemy)
    {
        if (attackSequence.isAlive && !isSubBehaviour)
            attackSequence.Stop();

        if (moveSequence.isAlive)
            moveSequence.Stop();

        if (circles != null && !circles.hasDetonated)
        {
            circles.Cancel();
            ReturnRocksToOrbiter();
        }
    }

    public void SetSubBehaviourState(bool state)
    {
        isSubBehaviour = state;
    }
}
