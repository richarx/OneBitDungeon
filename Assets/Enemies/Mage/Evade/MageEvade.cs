using Enemies.Scripts.Behaviours;
using Player.Scripts;
using PrimeTween;
using Tools_and_Scripts;
using UnityEngine;

public class MageEvade : MonoBehaviour, IEnemyBehaviour
{
    [SerializeField] private CircleDamageZone circleDamageZonePrefab;

    private CircleDamageZone circle;
    private Sequence currentSequence;
    private Sequence moveRockSequence;
    private Sequence detonateRockSequence;

    private SpinRock rock_1;
    private SpinRock rock_2;
    private SpinRock rock_3;

    public void StartBehaviour(EnemyController enemy)
    {
        Debug.Log("Mage EVADE");

        Vector3 currentPosition = enemy.transform.position;
        Vector3 playerPosition = PlayerStateMachine.instance.position;
        Vector3 targetPosition = playerPosition + (currentPosition - playerPosition).normalized * 0.5f;

        Vector3 evadePosition = targetPosition.magnitude <= 0.01f ? Vector3.forward * 7.0f : (targetPosition * -1.0f).normalized * 7.0f;

        float moveDuration = enemy.isSecondPhase ? 0.3f : 0.5f;

        rock_1 = RockOrbiter.instance.GetRandomRock();
        rock_2 = RockOrbiter.instance.GetRandomRock();
        rock_3 = RockOrbiter.instance.GetRandomRock();

        currentSequence = Sequence.Create()
            .ChainCallback(() => SpawnDamageZone(targetPosition))
            .ChainCallback(() =>
            {
                if (enemy.isSecondPhase)
                    enemy.afterImage.Trigger(moveDuration);
            })
            .ChainCallback(() => MoveRocksToStartingPosition(targetPosition))
            .Group(Tween.Position(enemy.transform, targetPosition, moveDuration, Ease.InOutCubic))
            .ChainCallback(() => enemy.animator.Play("Blast"))
            .ChainCallback(() => DetonateRocks(moveDuration))
            .Chain(Tween.ScaleX(enemy.transform, 0.0f, 0.3f, Ease.InBack))
            .ChainCallback(() => enemy.transform.position = evadePosition)
            .Chain(Tween.ScaleX(enemy.transform, 1.0f, 0.3f, Ease.OutBack))
            .ChainDelay(enemy.isSecondPhase ? 0.5f : 0.75f)
            .ChainCallback(() => enemy.SelectNewBehaviour());
    }

    private void SpawnDamageZone(Vector3 position)
    {
        circle = Instantiate(circleDamageZonePrefab, position, Quaternion.Euler(new Vector3(90.0f, 0.0f, 0.0f)));
        circle.Setup();
    }

    private void MoveRocksToStartingPosition(Vector3 targetPosition)
    {
        rock_1.SetLockState(true);
        rock_2.SetLockState(true);
        rock_3.SetLockState(true);

        Vector3 rockPosition = targetPosition + Vector3.up * 8.0f;
        float angle = Random.Range(0.0f, 360.0f);
        float distanceFromCenter = 1.5f;

        moveRockSequence = Sequence.Create()
            .Group(Tween.LocalPosition(rock_1.transform, rockPosition + Vector2.right.AddAngleToDirection(angle).ToVector3() * distanceFromCenter, 0.5f, Ease.InOutBack))
            .Group(Tween.LocalPosition(rock_2.transform, rockPosition + Vector2.right.AddAngleToDirection(angle + 120.0f).ToVector3() * distanceFromCenter, 0.5f, Ease.InOutBack))
            .Group(Tween.LocalPosition(rock_3.transform, rockPosition + Vector2.right.AddAngleToDirection(angle + 240.0f).ToVector3() * distanceFromCenter, 0.5f, Ease.InOutBack))
            ;
    }

    private void DetonateRocks(float moveDuration)
    {
        detonateRockSequence = Sequence.Create()
            .ChainDelay(1.0f - moveDuration - 0.2f)
            .Chain(Tween.LocalPositionY(rock_1.transform, 0.0f, 0.3f, Ease.InOutBack))
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
        SpawnSingleDebris(rock_1);
        SpawnSingleDebris(rock_2);
        SpawnSingleDebris(rock_3);
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
    }

    public void UpdateBehaviour(EnemyController enemy)
    {
    }

    public void FixedUpdateBehaviour(EnemyController enemy)
    {
    }

    public void StopBehaviour(EnemyController enemy)
    {
        if (currentSequence.isAlive)
            currentSequence.Stop();
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

        SpawnDebris();
        ReturnRocksToOrbiter();
    }

    public bool isSubBehaviour;
    public void SetSubBehaviourState(bool state)
    {
        isSubBehaviour = state;
    }
}
