using System;
using Enemies.Scripts.Behaviours;
using Player.Scripts;
using PrimeTween;
using Tools_and_Scripts;
using UnityEngine;

public class MageThrow : MonoBehaviour, IEnemyBehaviour
{
    [SerializeField] private float rotationDampening;
    [SerializeField] private float rotationDuration;
    [SerializeField] private GameObject rectangleDamageZonePrefab;

    private bool isSubBehaviour;

    private Transform rightThrow;
    private Vector3 rightDirection;
    private float rightThrowTimestamp;

    private Transform leftThrow;
    private Vector3 leftDirection;
    private float leftThrowTimestamp;

    private Sequence attackSequence;
    private Sequence moveSequence1;
    private Sequence moveSequence2;
    private Sequence detonationSequence1;
    private Sequence detonationSequence2;

    private SpinRock rock_1;
    private SpinRock rock_2;


    public void StartBehaviour(EnemyController enemy)
    {
        Debug.Log("Mage THROW");

        Vector3 enemyPosition = new Vector3(0.0f, 0.0f, 8.5f);
        Vector3 rightPosition = new Vector3(3.0f, 0.0f, 9.0f);
        Vector3 leftPosition = new Vector3(-3.0f, 0.0f, 9.0f);

        float moveDuration = enemy.isSecondPhase ? 0.5f : 1.0f;

        rock_1 = RockOrbiter.instance.GetRandomRock();
        rock_2 = RockOrbiter.instance.GetRandomRock();

        attackSequence = Sequence.Create()
            .ChainCallback(() =>
            {
                if (enemy.isSecondPhase)
                    enemy.afterImage.Trigger(moveDuration);
                MageSFX.instance.PlayMageMove();
            })
            .Group(Tween.Position(enemy.transform, enemyPosition, moveDuration, Ease.InOutCubic))
            .ChainCallback(() =>
            {
                enemy.animator.Play("Cast");
                rightThrow = SpawnDamageZone(rightPosition);
                rightThrowTimestamp = Time.time;
            })
            .ChainCallback(() => moveSequence1 = MoveRockToStartingPosition(rock_1, rightPosition))
            .ChainDelay(0.6f)
            .ChainCallback(() =>
            {
                enemy.animator.Play("Cast_Left");
                leftThrow = SpawnDamageZone(leftPosition);
                leftThrowTimestamp = Time.time;
            })
            .ChainCallback(() => moveSequence2 = MoveRockToStartingPosition(rock_2, leftPosition))
            .ChainDelay(0.9f)
            .ChainCallback(() => detonationSequence1 = DetonateRock(rock_1, rightPosition, true, enemy.animator))
            .ChainDelay(0.6f)
            .ChainCallback(() => detonationSequence2 = DetonateRock(rock_2, leftPosition, false, enemy.animator))
            .ChainDelay(0.6f)
            .ChainCallback(() => enemy.SelectNewBehaviour());
    }

    private Sequence MoveRockToStartingPosition(SpinRock rock, Vector3 position)
    {
        rock.SetLockState(true);

        MageSFX.instance.PlayRockMove();

        return Sequence.Create()
            .Chain(Tween.LocalPosition(rock.transform, position, 0.5f, Ease.OutBack));
    }

    private Sequence DetonateRock(SpinRock rock, Vector3 position, bool isRight, Animator animator)
    {
        float targetDistance = 20.0f;
        Vector3 punchScale = new Vector3(1.3f, 0.7f, 1.0f);
        Vector3 direction = isRight ?
            rightDirection.normalized.ToVector2().AddAngleToDirection(-90.0f).ToVector3() :
            leftDirection.normalized.ToVector2().AddAngleToDirection(-90.0f).ToVector3();

        Vector3 target1 = position + direction * targetDistance;

        animator.Play(isRight ? "Shoot_Right" : "Shoot_Left");
        MageSFX.instance.PlayRockThrow();

        return Sequence.Create()
                    .Chain(Tween.LocalPosition(rock.transform, target1, 0.5f, Ease.InOutBack))
                    .Group(Tween.PunchScale(rock.transform, punchScale, 0.1f, 3.0f, startDelay: 0.5f))
                    .ChainCallback(() => SpawnDebris(rock))
                    .ChainCallback(() => ReturnRocksToOrbiter(isRight))
                    ;
    }

    private void ReturnRocksToOrbiter(bool isRight)
    {
        if (isRight && rock_1 != null)
        {
            RockOrbiter.instance.AddBackRock(rock_1);
            rock_1 = null;
        }

        if (!isRight && rock_2 != null)
        {
            RockOrbiter.instance.AddBackRock(rock_2);
            rock_2 = null;
        }
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
        if (attackSequence.isAlive)
            attackSequence.Stop();
    }

    public void CancelBehaviour(EnemyController enemy)
    {
        if (attackSequence.isAlive)
            attackSequence.Stop();

        if (moveSequence1.isAlive)
            moveSequence1.Stop();

        if (moveSequence2.isAlive)
            moveSequence2.Stop();

        if (detonationSequence1.isAlive)
            detonationSequence1.Stop();

        if (detonationSequence2.isAlive)
            detonationSequence2.Stop();

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
        isSubBehaviour = state;
    }
}
