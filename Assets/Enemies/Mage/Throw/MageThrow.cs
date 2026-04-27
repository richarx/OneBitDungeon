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
    private float rightThrowTimestamp;

    private Transform leftThrow;
    private float leftThrowTimestamp;

    private Sequence attackSequence;

    public void StartBehaviour(EnemyController enemy)
    {
        Debug.Log("Mage THROW");

        Vector3 enemyPosition = new Vector3(0.0f, 0.0f, 8.5f);
        Vector3 rightPosition = new Vector3(3.0f, 0.0f, 9.0f);
        Vector3 leftPosition = new Vector3(-3.0f, 0.0f, 9.0f);

        float moveDuration = enemy.isSecondPhase ? 0.5f : 1.0f;

        attackSequence = Sequence.Create()
            .ChainCallback(() =>
            {
                if (enemy.isSecondPhase)
                    enemy.afterImage.Trigger(moveDuration);
            })
            .Group(Tween.Position(enemy.transform, enemyPosition, moveDuration, Ease.InOutCubic))
            .ChainCallback(() =>
            {
                enemy.animator.Play("Shoot_Right");
                rightThrow = SpawnDamageZone(rightPosition);
                rightThrowTimestamp = Time.time;
            })
            .ChainDelay(0.6f)
            .ChainCallback(() =>
            {
                enemy.animator.Play("Shoot_Left");
                leftThrow = SpawnDamageZone(leftPosition);
                leftThrowTimestamp = Time.time;
            })
            .ChainDelay(enemy.isSecondPhase ? 0.5f : 1.5f)
            .ChainCallback(() => enemy.SelectNewBehaviour());
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
            RotateThrowTowardPlayer(rightThrow);

        if (leftThrow != null && Time.time - leftThrowTimestamp <= rotationDuration)
            RotateThrowTowardPlayer(leftThrow);
    }

    private void RotateThrowTowardPlayer(Transform rectangle)
    {
        Vector3 position = rectangle.position;
        Vector3 direction = (PlayerStateMachine.instance.position - position).normalized.ToVector2().AddAngleToDirection(90.0f).ToVector3();

        rectangle.rotation = Quaternion.Slerp(rectangle.rotation, Quaternion.LookRotation(direction), Time.deltaTime / rotationDampening);
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
