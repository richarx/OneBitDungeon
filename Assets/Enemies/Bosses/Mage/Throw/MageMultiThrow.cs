using Enemies.Scripts.Behaviours;
using Player.Scripts;
using PrimeTween;
using Tools_and_Scripts;
using UnityEngine;

public class MageMultiThrow : MonoBehaviour, IEnemyBehaviour
{
    [SerializeField] private float rotationDampening;
    [SerializeField] private float rotationDuration;
    [SerializeField] private GameObject rectangleDamageZonePrefab;

    private Transform rightThrow;
    private Vector3 rightDirection;
    private float rightThrowTimestamp;

    private Transform leftThrow;
    private Vector3 leftDirection;
    private float leftThrowTimestamp;

    private Sequence mainSequence;
    private Sequence attackSequence;
    private Sequence moveSequence;

    private SpinRock rock_1;
    private SpinRock rock_2;

    public void StartBehaviour(EnemyController enemy)
    {
        Debug.Log("Mage THROW");

        Vector3 enemyPosition = new Vector3(0.0f, 0.0f, 8.5f);
        Vector3 enemyRightPosition = new Vector3(6.0f, 0.0f, 8.5f);
        Vector3 enemyLeftPosition = new Vector3(-6.0f, 0.0f, 8.5f);

        mainSequence = Sequence.Create()
            .ChainCallback(() => MoveBossToPosition(enemy, enemyPosition, 0.3f))
            .ChainCallback(() => Throw2Rocks(enemy, enemyPosition, 0.3f))
            .ChainDelay(1.85f)
            .ChainCallback(() => MoveBossToPosition(enemy, enemyRightPosition, 0.1f))
            .ChainCallback(() => Throw2Rocks(enemy, enemyRightPosition, 0.1f))
            .ChainDelay(1.65f)
            .ChainCallback(() => MoveBossToPosition(enemy, enemyLeftPosition, 0.1f))
            .ChainCallback(() => Throw2Rocks(enemy, enemyLeftPosition, 0.1f))
            .ChainDelay(1.65f)
            .ChainCallback(() => enemy.SelectNewBehaviour());
    }

    private void MoveBossToPosition(EnemyController enemy, Vector3 enemyPosition, float moveDuration)
    {
        attackSequence = Sequence.Create()
            .ChainCallback(() =>
            {
                enemy.afterImage.Trigger(moveDuration);
                MageSFX.instance.PlayMageMove();
            })
            .Group(Tween.Position(enemy.transform, enemyPosition, moveDuration, Ease.InOutCubic));
    }

    private void Throw2Rocks(EnemyController enemy, Vector3 enemyPosition, float delay)
    {
        rock_1 = RockOrbiter.instance.GetRandomRock();
        rock_2 = RockOrbiter.instance.GetRandomRock();

        attackSequence = Sequence.Create()
            .ChainDelay(delay)
            .ChainCallback(() =>
            {
                enemy.animator.Play("Cast");
                rightThrow = SpawnDamageZone(enemyPosition + Vector3.right * 3.0f);
                rightThrowTimestamp = Time.time;
            })
            .ChainCallback(() => MoveRockToStartingPosition(rock_1, enemyPosition + Vector3.right * 3.0f))
            .ChainDelay(0.3f)
            .ChainCallback(() =>
            {
                enemy.animator.Play("Cast_Left");
                leftThrow = SpawnDamageZone(enemyPosition - Vector3.right * 3.0f);
                leftThrowTimestamp = Time.time;
            })
            .ChainCallback(() => MoveRockToStartingPosition(rock_2, enemyPosition - Vector3.right * 3.0f))
            .ChainDelay(0.45f)
            .ChainCallback(() => DetonateRock(rock_1, enemyPosition + Vector3.right * 3.0f, true, enemy.animator))
            .ChainDelay(0.3f)
            .ChainCallback(() => DetonateRock(rock_2, enemyPosition - Vector3.right * 3.0f, false, enemy.animator))
            ;
    }

    private void MoveRockToStartingPosition(SpinRock rock, Vector3 position)
    {
        rock.SetState(SpinRock.RockState.Locked);

        MageSFX.instance.PlayRockMove();

        Sequence.Create()
            .Chain(Tween.LocalPosition(rock.transform, position, 0.5f, Ease.OutBack));
    }

    private void DetonateRock(SpinRock rock, Vector3 position, bool isRight, Animator animator)
    {
        float targetDistance = 20.0f;
        Vector3 punchScale = new Vector3(1.3f, 0.7f, 1.0f);
        Vector3 direction = isRight ?
            rightDirection.normalized.ToVector2().AddAngleToDirection(-90.0f).ToVector3() :
            leftDirection.normalized.ToVector2().AddAngleToDirection(-90.0f).ToVector3();

        Vector3 target1 = position + direction * targetDistance;

        animator.Play(isRight ? "Shoot_Right" : "Shoot_Left");
        MageSFX.instance.PlayRockThrow();

        Sequence.Create()
            .Chain(Tween.LocalPosition(rock.transform, target1, 0.5f, Ease.InOutBack))
            .Group(Tween.PunchScale(rock.transform, punchScale, 0.1f, 3.0f, startDelay: 0.5f))
            .ChainCallback(() => SpawnDebris(rock))
            .ChainCallback(() => ReturnRocksToOrbiter(rock))
            ;
    }

    private void ReturnRocksToOrbiter(SpinRock rock)
    {
        if (rock != null)
            RockOrbiter.instance.AddBackRock(rock);
    }

    private void SpawnDebris(SpinRock rock)
    {
        if (rock != null)
            RockOrbiter.instance.SpawnDebris(rock.transform.position);
    }

    private Transform SpawnDamageZone(Vector3 position)
    {
        Transform rectangle = Instantiate(rectangleDamageZonePrefab, position, Quaternion.identity).transform;
        rectangle.GetChild(0).GetComponent<RectangleDamageZone>().Setup(Vector2.right);

        return rectangle;
    }

    public void UpdateBehaviour(EnemyController enemy)
    {
        if (rightThrow != null && Time.time - rightThrowTimestamp <= rotationDuration)
            rightDirection = RotateThrowTowardPlayer(rightThrow);

        if (leftThrow != null && Time.time - leftThrowTimestamp <= rotationDuration)
            leftDirection = RotateThrowTowardPlayer(leftThrow);
    }

    private Vector3 RotateThrowTowardPlayer(Transform rectangle)
    {
        Vector3 position = rectangle.position;
        Vector3 direction = (PlayerStateMachine.instance.position - position).normalized.ToVector2().AddAngleToDirection(90.0f).ToVector3();

        rectangle.rotation = Quaternion.Slerp(rectangle.rotation, Quaternion.LookRotation(direction), Time.deltaTime / rotationDampening);

        return direction;
    }

    public void FixedUpdateBehaviour(EnemyController enemy)
    {
    }

    public void StopBehaviour(EnemyController enemy)
    {
    }

    public void CancelBehaviour(EnemyController enemy)
    {
        if (mainSequence.isAlive)
            mainSequence.Stop();

        if (attackSequence.isAlive)
            attackSequence.Stop();

        if (rightThrow != null)
            rightThrow.GetChild(0).GetComponent<RectangleDamageZone>().Cancel();

        if (leftThrow != null)
            leftThrow.GetChild(0).GetComponent<RectangleDamageZone>().Cancel();

        SpawnDebris(rock_1);
        SpawnDebris(rock_2);
        ReturnRocksToOrbiter(rock_1);
        ReturnRocksToOrbiter(rock_2);
    }

    public void SetSubBehaviourState(bool state)
    {
    }
}
