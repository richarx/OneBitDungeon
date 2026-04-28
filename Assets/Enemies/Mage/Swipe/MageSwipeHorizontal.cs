using System.Collections.Generic;
using Enemies.Scripts.Behaviours;
using PrimeTween;
using UnityEngine;

public class MageSwipeHorizontal : MonoBehaviour, IEnemyBehaviour
{
    [SerializeField] private RectangleDamageZone rectangleDamageZonePrefab;

    private bool isSubBehaviour;

    private List<RectangleDamageZone> rectangles;
    private Sequence attackSequence;
    private Sequence moveSequence;
    private Sequence detonationSequence;
    private List<Sequence> moveRockSequences;

    private SpinRock rock_1;
    private SpinRock rock_2;
    private SpinRock rock_3;
    private SpinRock rock_4;
    private SpinRock rock_5;

    public void StartBehaviour(EnemyController enemy)
    {
        Debug.Log("Mage SWIPE HORIZONTAL");

        rectangles = new List<RectangleDamageZone>();
        moveRockSequences = new List<Sequence>();

        Vector3 randomPosition = Random.insideUnitSphere * 7.0f;
        randomPosition.y = 0.0f;

        float moveDuration = enemy.isSecondPhase ? 0.5f : 1.0f;

        if (!isSubBehaviour)
        {
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

        rock_1 = RockOrbiter.instance.GetRandomRock();
        rock_2 = RockOrbiter.instance.GetRandomRock();
        rock_3 = RockOrbiter.instance.GetRandomRock();
        rock_4 = RockOrbiter.instance.GetRandomRock();
        rock_5 = RockOrbiter.instance.GetRandomRock();

        attackSequence = Sequence.Create()
            .ChainCallback(() => SpawnDamageZone(new Vector3(10.0f, 0.0f, 8.92f), Vector2.left))
            .ChainCallback(() => MoveRockToStartingPosition(rock_1, new Vector3(10.0f, 0.0f, 8.92f)))
            .ChainDelay(0.05f)
            .ChainCallback(() => SpawnDamageZone(new Vector3(-10.0f, 0.0f, 4.42f), Vector2.right))
            .ChainCallback(() => MoveRockToStartingPosition(rock_2, new Vector3(-10.0f, 0.0f, 4.42f)))
            .ChainDelay(0.05f)
            .ChainCallback(() => SpawnDamageZone(new Vector3(10.0f, 0.0f, 0.0f), Vector2.left))
            .ChainCallback(() => MoveRockToStartingPosition(rock_3, new Vector3(10.0f, 0.0f, 0.0f)))
            .ChainDelay(0.05f)
            .ChainCallback(() => SpawnDamageZone(new Vector3(-10.0f, 0.0f, -4.58f), Vector2.right))
            .ChainCallback(() => MoveRockToStartingPosition(rock_4, new Vector3(-10.0f, 0.0f, -4.58f)))
            .ChainDelay(0.05f)
            .ChainCallback(() => SpawnDamageZone(new Vector3(10.0f, 0.0f, -9.11f), Vector2.left))
            .ChainCallback(() => MoveRockToStartingPosition(rock_5, new Vector3(10.0f, 0.0f, -9.11f)))
            .ChainCallback(() => DetonateRocks())
            .ChainDelay(enemy.isSecondPhase ? 1.3f : 2.5f)
            .ChainCallback(() =>
            {
                if (!isSubBehaviour)
                    enemy.SelectNewBehaviour();
            });
    }

    private void DetonateRocks()
    {
        float targetDistance = 12.0f;
        Vector3 punchScale = new Vector3(0.7f, 1.3f, 1.0f);

        detonationSequence = Sequence.Create()
            .ChainDelay(1.3f)
            .Chain(Tween.LocalPositionX(rock_1.transform, -targetDistance, 0.5f, Ease.InOutBack))
            .Group(Tween.LocalPositionX(rock_2.transform, targetDistance, 0.5f, Ease.InOutBack, startDelay: 0.05f))
            .Group(Tween.LocalPositionX(rock_3.transform, -targetDistance, 0.5f, Ease.InOutBack, startDelay: 0.1f))
            .Group(Tween.LocalPositionX(rock_4.transform, targetDistance, 0.5f, Ease.InOutBack, startDelay: 0.15f))
            .Group(Tween.LocalPositionX(rock_5.transform, -targetDistance, 0.5f, Ease.InOutBack, startDelay: 0.2f))
            .Chain(Tween.PunchScale(rock_1.transform, punchScale, 0.1f, 3.0f))
            .Group(Tween.PunchScale(rock_2.transform, punchScale, 0.1f, 3.0f))
            .Group(Tween.PunchScale(rock_3.transform, punchScale, 0.1f, 3.0f))
            .Group(Tween.PunchScale(rock_4.transform, punchScale, 0.1f, 3.0f))
            .Group(Tween.PunchScale(rock_5.transform, punchScale, 0.1f, 3.0f))
            .ChainCallback(() => SpawnDebris())
            .ChainCallback(() => ReturnRocksToOrbiter())
            ;
    }

    private void SpawnDebris()
    {
        SpawnSingleDebris(rock_1);
        SpawnSingleDebris(rock_2);
        SpawnSingleDebris(rock_3);
        SpawnSingleDebris(rock_4);
        SpawnSingleDebris(rock_5);
    }

    private void SpawnSingleDebris(SpinRock rock)
    {
        if (rock != null)
            RockOrbiter.instance.SpawnDebris(rock.transform.position);
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

        if (rock_4 != null)
        {
            RockOrbiter.instance.AddBackRock(rock_4);
            rock_4 = null;
        }

        if (rock_5 != null)
        {
            RockOrbiter.instance.AddBackRock(rock_5);
            rock_5 = null;
        }
    }

    private void MoveRockToStartingPosition(SpinRock rock, Vector3 position)
    {
        rock.SetLockState(true);

        moveRockSequences.Add(Sequence.Create()
            .Chain(Tween.LocalPosition(rock.transform, position, 0.5f, Ease.OutBack)));
    }

    private void SpawnDamageZone(Vector3 position, Vector2 direction)
    {
        RectangleDamageZone rectangle = Instantiate(rectangleDamageZonePrefab, position, Quaternion.Euler(new Vector3(90.0f, 0.0f, 0.0f)));
        rectangle.Setup(direction);

        rectangles.Add(rectangle);
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
    }

    public void CancelBehaviour(EnemyController enemy)
    {
        if (attackSequence.isAlive)
            attackSequence.Stop();

        if (moveSequence.isAlive)
            moveSequence.Stop();

        if (detonationSequence.isAlive)
            detonationSequence.Stop();

        foreach (RectangleDamageZone rectangle in rectangles)
        {
            if (rectangle != null)
                rectangle.Cancel();
        }

        foreach (Sequence moveRock in moveRockSequences)
        {
            if (moveRock.isAlive)
                moveRock.Stop();
        }

        SpawnDebris();
        ReturnRocksToOrbiter();
    }

    public void SetSubBehaviourState(bool state)
    {
        isSubBehaviour = state;
    }
}
