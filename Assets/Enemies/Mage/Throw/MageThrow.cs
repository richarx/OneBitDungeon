using System;
using Enemies.Scripts.Behaviours;
using Player.Scripts;
using PrimeTween;
using Tools_and_Scripts;
using UnityEngine;

public class MageThrow : MonoBehaviour, IEnemyBehaviour
{
    [SerializeField] private float rotationDampening;
    [SerializeField] private float fastRotationDampening;
    [SerializeField] private float rotationDuration;
    [SerializeField] private GameObject rectangleDamageZonePrefab;
    [SerializeField] private GameObject secondPhaseRectangleDamageZonePrefab;

    private bool isSubBehaviour;

    private Transform rightThrow;
    private Vector3 rightDirection;
    private float rightThrowTimestamp;

    private Transform leftThrow;
    private Vector3 leftDirection;
    private float leftThrowTimestamp;

    private Sequence attackSequence;

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
            })
            .Group(Tween.Position(enemy.transform, enemyPosition, moveDuration, Ease.InOutCubic))
            .ChainCallback(() =>
            {
                enemy.animator.Play("Cast");
                rightThrow = SpawnDamageZone(rightPosition, enemy.isSecondPhase);
                rightThrowTimestamp = Time.time;
            })
            .ChainCallback(() => MoveRockToStartingPosition(rock_1, rightPosition))
            .ChainDelay(enemy.isSecondPhase ? 0.3f : 0.6f)
            .ChainCallback(() =>
            {
                enemy.animator.Play("Cast_Left");
                leftThrow = SpawnDamageZone(leftPosition, enemy.isSecondPhase);
                leftThrowTimestamp = Time.time;
            })
            .ChainCallback(() => MoveRockToStartingPosition(rock_2, leftPosition))
            .ChainDelay(enemy.isSecondPhase ? 0.45f : 0.9f)
            .ChainCallback(() => DetonateRock(rock_1, rightPosition, true, enemy.animator))
            .ChainDelay(enemy.isSecondPhase ? 0.3f : 0.6f)
            .ChainCallback(() => DetonateRock(rock_2, leftPosition, false, enemy.animator))
            .ChainDelay(enemy.isSecondPhase ? 0.3f : 0.6f)
            .ChainCallback(() => enemy.SelectNewBehaviour());
    }

    private void MoveRockToStartingPosition(SpinRock rock, Vector3 position)
    {
        rock.SetLockState(true);

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

        Sequence.Create()
                    .Chain(Tween.LocalPosition(rock.transform, target1, 0.5f, Ease.InOutBack))
                    .Chain(Tween.PunchScale(rock.transform, punchScale, 0.1f, 3.0f, startDelay: 0.5f))
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
        RockOrbiter.instance.SpawnDebris(rock.transform.position);
    }

    private Transform SpawnDamageZone(Vector3 position, bool isSecondPhase)
    {
        Transform rectangle = Instantiate(isSecondPhase ? secondPhaseRectangleDamageZonePrefab : rectangleDamageZonePrefab, position, Quaternion.identity).transform;
        rectangle.GetChild(0).GetComponent<RectangleDamageZone>().Setup(Vector2.right);

        return rectangle;
    }

    public void UpdateBehaviour(EnemyController enemy)
    {
        if (rightThrow != null && Time.time - rightThrowTimestamp <= rotationDuration)
            rightDirection = RotateThrowTowardPlayer(rightThrow, enemy.isSecondPhase ? fastRotationDampening : rotationDampening);

        if (leftThrow != null && Time.time - leftThrowTimestamp <= rotationDuration)
            leftDirection = RotateThrowTowardPlayer(leftThrow, enemy.isSecondPhase ? fastRotationDampening : rotationDampening);
    }

    private Vector3 RotateThrowTowardPlayer(Transform rectangle, float dampening)
    {
        Vector3 position = rectangle.position;
        Vector3 direction = (PlayerStateMachine.instance.position - position).normalized.ToVector2().AddAngleToDirection(90.0f).ToVector3();

        rectangle.rotation = Quaternion.Slerp(rectangle.rotation, Quaternion.LookRotation(direction), Time.deltaTime / dampening);

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

    public void SetSubBehaviourState(bool state)
    {
        isSubBehaviour = state;
    }
}
