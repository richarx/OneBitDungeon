using System;
using System.Collections.Generic;
using PrimeTween;
using Tools_and_Scripts;
using UnityEngine;

public class MageEvadeSpell : MonoBehaviour
{
    [SerializeField] private Projectile projectilePrefab;

    private List<Projectile> projectiles = new List<Projectile>();

    private CircleDamageZone circle;
    private Sequence sequence;

    public void Setup(float _radius, float _spawnDuration, float _fillDuration, Action onShootCallback)
    {
        CircleDamageZone circle = GetComponent<CircleDamageZone>();
        circle.Setup(_radius, _spawnDuration, _fillDuration);

        sequence = Sequence.Create()
            .ChainCallback(() => SpawnRocks())
            .ChainDelay(_spawnDuration + _fillDuration - 0.15f)
            .ChainCallback(() =>
            {
                ShootRocks();
                onShootCallback?.Invoke();
            });
    }

    public void Cancel()
    {
        if (circle != null)
            circle.Cancel();

        if (sequence.isAlive)
            sequence.Stop();

        foreach (Projectile projectile in projectiles)
            projectile.CancelProjectile();
    }

    private void ShootRocks()
    {
        foreach (Projectile projectile in projectiles)
            projectile.Shoot(ComputeRockTargetPosition(projectile.transform.position), 0.3f);
    }

    private void SpawnRocks()
    {
        Vector3 position = transform.position;

        for (int i = 0; i < 3; i++)
        {
            Vector3 hoverPosition = ComputeRockPosition(position, i);

            Projectile projectile = Instantiate(projectilePrefab, hoverPosition + Vector3.up * 5.0f, Quaternion.identity);
            projectile.MoveToStartingPosition(hoverPosition, 0.3f);

            projectiles.Add(projectile);
        }

        MageSFX.instance.PlayRockMove();
    }

    private Vector3 ComputeRockPosition(Vector3 position, int index)
    {
        float z = Mathf.Clamp(position.z, -10.0f, 0.0f);
        float height = 8.0f + 4.0f * Tools.NormalizeValue(z, -10.0f, 0.0f);

        Vector3 rockPosition = position + Vector3.up * height;
        float angle = UnityEngine.Random.Range(0.0f, 360.0f);
        float distanceFromCenter = 1.5f;

        return rockPosition + Vector2.right.AddAngleToDirection(angle + (index * 120.0f)).ToVector3() * distanceFromCenter;
    }

    private Vector3 ComputeRockTargetPosition(Vector3 position)
    {
        position.y = 0.0f;
        return position;
    }
}
