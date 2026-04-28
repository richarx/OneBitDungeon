using Enemies.Scripts.Behaviours;
using PrimeTween;
using UnityEngine;

public class MageSwipeVertical : MonoBehaviour, IEnemyBehaviour
{
    [SerializeField] private RectangleDamageZone rectangleDamageZonePrefab;

    private bool isSubBehaviour;

    private RectangleDamageZone rectangle;
    private Sequence attackSequence;
    private Sequence moveSequence;

    private SpinRock rock_1;
    private SpinRock rock_2;
    private SpinRock rock_3;
    private SpinRock rock_4;
    private SpinRock rock_5;

    public void StartBehaviour(EnemyController enemy)
    {
        Debug.Log("Mage SWIPE VERTICAL");

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
            .ChainCallback(() => SpawnDamageZone(new Vector3(8.92f, 0.0f, 10.0f), Vector2.down))
            .ChainCallback(() => MoveRockToStartingPosition(rock_1, new Vector3(8.92f, 0.0f, 10.0f)))
            .ChainDelay(0.05f)
            .ChainCallback(() => SpawnDamageZone(new Vector3(4.42f, 0.0f, -10.0f), Vector2.up))
            .ChainCallback(() => MoveRockToStartingPosition(rock_2, new Vector3(4.42f, 0.0f, -10.0f)))
            .ChainDelay(0.05f)
            .ChainCallback(() => SpawnDamageZone(new Vector3(0.0f, 0.0f, 10.0f), Vector2.down))
            .ChainCallback(() => MoveRockToStartingPosition(rock_3, new Vector3(0.0f, 0.0f, 10.0f)))
            .ChainDelay(0.05f)
            .ChainCallback(() => SpawnDamageZone(new Vector3(-4.58f, 0.0f, -10.0f), Vector2.up))
            .ChainCallback(() => MoveRockToStartingPosition(rock_4, new Vector3(-4.58f, 0.0f, -10.0f)))
            .ChainDelay(0.05f)
            .ChainCallback(() => SpawnDamageZone(new Vector3(-9.11f, 0.0f, 10.0f), Vector2.down))
            .ChainCallback(() => MoveRockToStartingPosition(rock_5, new Vector3(-9.11f, 0.0f, 10.0f)))
            .ChainCallback(() => DetonateRocks())
            .ChainDelay(enemy.isSecondPhase ? 0.5f : 2.5f)
            .ChainCallback(() =>
            {
                if (!isSubBehaviour)
                    enemy.SelectNewBehaviour();
            });
    }

    private void DetonateRocks()
    {
        float targetDistance = 12.0f;
        Vector3 punchScale = new Vector3(1.3f, 0.7f, 1.0f);

        Sequence.Create()
            .ChainDelay(1.3f)
            .Chain(Tween.LocalPositionZ(rock_1.transform, -targetDistance, 0.5f, Ease.InOutBack))
            .Group(Tween.LocalPositionZ(rock_2.transform, targetDistance, 0.5f, Ease.InOutBack, startDelay: 0.05f))
            .Group(Tween.LocalPositionZ(rock_3.transform, -targetDistance, 0.5f, Ease.InOutBack, startDelay: 0.1f))
            .Group(Tween.LocalPositionZ(rock_4.transform, targetDistance, 0.5f, Ease.InOutBack, startDelay: 0.15f))
            .Group(Tween.LocalPositionZ(rock_5.transform, -targetDistance, 0.5f, Ease.InOutBack, startDelay: 0.2f))
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
        RockOrbiter.instance.SpawnDebris(rock_1.transform.position);
        RockOrbiter.instance.SpawnDebris(rock_2.transform.position);
        RockOrbiter.instance.SpawnDebris(rock_3.transform.position);
        RockOrbiter.instance.SpawnDebris(rock_4.transform.position);
        RockOrbiter.instance.SpawnDebris(rock_5.transform.position);
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

        Sequence.Create()
            .Chain(Tween.LocalPosition(rock.transform, position, 0.5f, Ease.OutBack));
    }

    private void SpawnDamageZone(Vector3 position, Vector2 direction)
    {
        rectangle = Instantiate(rectangleDamageZonePrefab, position, Quaternion.Euler(new Vector3(90.0f, 0.0f, 0.0f)));
        rectangle.Setup(direction);
    }

    public void UpdateBehaviour(EnemyController enemy)
    {
    }

    public void FixedUpdateBehaviour(EnemyController enemy)
    {
    }

    public void StopBehaviour(EnemyController enemy)
    {
        if (attackSequence.isAlive)
            attackSequence.Stop();

        if (moveSequence.isAlive)
            moveSequence.Stop();
    }

    public void SetSubBehaviourState(bool state)
    {
        isSubBehaviour = state;
    }
}
