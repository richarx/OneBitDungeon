using System.Collections.Generic;
using Player.Scripts;
using PrimeTween;
using Tools_and_Scripts;
using UnityEngine;

public class MageRainSpell : MonoBehaviour
{
    [SerializeField] private Projectile projectilePrefab;

    private ThreeCirclesDamageZone threeCirclesDamageZone;
    private Sequence sequence;

    private List<Projectile> projectiles = new List<Projectile>();

    public void Setup(float radius, float spawnDuration, float fillDuration)
    {
        threeCirclesDamageZone = GetComponent<ThreeCirclesDamageZone>();
        threeCirclesDamageZone.Setup(radius, spawnDuration, fillDuration);

        sequence = Sequence.Create()
            .ChainCallback(() => SetupCircle(0, ComputeTargetPosition(0), radius, spawnDuration, fillDuration))
            .ChainDelay(0.5f)
            .ChainCallback(() => SetupCircle(1, ComputeTargetPosition(1), radius, spawnDuration, fillDuration))
            .ChainDelay(0.5f)
            .ChainCallback(() => SetupCircle(2, ComputeTargetPosition(2), radius, spawnDuration, fillDuration))
            .ChainDelay(0.5f)
            .ChainCallback(() => Detonate());
    }

    private Vector3 ComputeTargetPosition(int index)
    {
        Vector3 position = PlayerStateMachine.instance.position;

        if (index == 0)
            return position;

        Vector3 previousPosition = ComputePreviousPosition(index, position);

        Vector3 direction = position - previousPosition;
        float distance = direction.magnitude;
        direction = direction.normalized;

        if (distance <= 1.5f)
        {
            if (distance <= 0.05f)
                return position + Random.insideUnitCircle.ToVector3().normalized * 1.5f;
            else
                return position + direction * 1.5f;
        }

        return position;
    }

    private Vector3 ComputePreviousPosition(int index, Vector3 position)
    {
        if (index < 2)
            return threeCirclesDamageZone.circlePosition1;

        float distance1 = Vector3.Distance(position, threeCirclesDamageZone.circlePosition1);
        float distance2 = Vector3.Distance(position, threeCirclesDamageZone.circlePosition2);

        return distance1 < distance2 ? threeCirclesDamageZone.circlePosition1 : threeCirclesDamageZone.circlePosition2;
    }

    public void Cancel()
    {
        if (threeCirclesDamageZone != null)
            threeCirclesDamageZone.Cancel();

        if (sequence.isAlive)
            sequence.Stop();

        foreach (Projectile projectile in projectiles)
            projectile.CancelProjectile();
    }

    private void SetupCircle(int index, Vector3 position, float radius, float spawnDuration, float fillDuration)
    {
        threeCirclesDamageZone.Setup(index);
        threeCirclesDamageZone.MoveCircle(index, position.ToVector2());

        Vector3 hoverPosition = ComputeRockHoverPosition(position);

        Projectile projectile = Instantiate(projectilePrefab, hoverPosition + Vector3.up * 5.0f, Quaternion.identity);
        projectile.MoveToStartingPosition(hoverPosition, 0.3f);

        MageSFX.instance.PlayRockMove();

        projectiles.Add(projectile);
    }

    private void Detonate()
    {
        threeCirclesDamageZone.Detonate();

        foreach (Projectile projectile in projectiles)
            projectile.Shoot(ComputeRockTargetPosition(projectile.transform.position), 0.3f);
    }

    private Vector3 ComputeRockTargetPosition(Vector3 position)
    {
        position.y = 0.0f;
        return position;
    }

    private Vector3 ComputeRockHoverPosition(Vector3 position)
    {
        float z = Mathf.Clamp(position.z, -10.0f, 0.0f);
        float height = 8.0f + 4.0f * Tools.NormalizeValue(z, -10.0f, 0.0f);

        return position + Vector3.up * height;
    }
}
