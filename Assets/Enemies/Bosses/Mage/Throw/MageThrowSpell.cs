using System;
using Player.Scripts;
using PrimeTween;
using Tools_and_Scripts;
using UnityEngine;

public class MageThrowSpell : MonoBehaviour
{
    [SerializeField] private Projectile projectilePrefab;

    private float rotationDuration;
    private float rotationDampening;
    private float throwTimestamp;

    private Sequence sequence;
    private RectangleDamageZone rectangleDamageZone;
    private Projectile projectile;

    public Vector3 rotationDirection { get; private set; }

    public void Setup(float duration, float dampening, float rockMovementDuration, float spawnDuration, float fillDuration, Action onShootCallback)
    {
        rotationDuration = duration;
        rotationDampening = dampening;
        throwTimestamp = Time.time;

        rectangleDamageZone = transform.GetChild(0).GetComponent<RectangleDamageZone>();
        rectangleDamageZone.Setup(Vector2.right, spawnDuration, fillDuration);

        Vector3 startingPosition = transform.position;

        projectile = Instantiate(projectilePrefab, startingPosition + Vector3.forward * -2.0f, Quaternion.identity);
        projectile.MoveToStartingPosition(startingPosition, 0.3f);

        sequence = Sequence.Create()
            .ChainDelay(spawnDuration)
            .ChainDelay(fillDuration)
            .ChainCallback(() => onShootCallback?.Invoke())
            .ChainCallback(() => projectile.Shoot(ComputeProjectileTargetPosition(startingPosition), rockMovementDuration));
    }

    public void Cancel()
    {
        rectangleDamageZone.Cancel();

        if (sequence.isAlive)
            sequence.Stop();

        if (projectile != null)
            projectile.CancelProjectile();

        throwTimestamp = -1000.0f;
    }

    private Vector3 ComputeProjectileTargetPosition(Vector3 startingPosition)
    {
        float targetDistance = 20.0f;
        Vector3 direction = rotationDirection.ToVector2().AddAngleToDirection(-90.0f).ToVector3();

        return startingPosition + direction * targetDistance;
    }

    public void Update()
    {
        if (Time.time - throwTimestamp <= rotationDuration)
            rotationDirection = RotateThrowTowardPlayer();
    }

    private Vector3 RotateThrowTowardPlayer()
    {
        Vector3 position = transform.position;
        Vector3 direction = (PlayerStateMachine.instance.position - position).normalized.ToVector2().AddAngleToDirection(90.0f).ToVector3();

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime / rotationDampening);

        return direction;
    }
}
