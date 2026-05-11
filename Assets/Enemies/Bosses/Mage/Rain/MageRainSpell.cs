using System.Collections.Generic;
using Player.Scripts;
using PrimeTween;
using Tools_and_Scripts;
using UnityEngine;

public class MageRainSpell : MonoBehaviour
{
    [SerializeField] private Projectile projectilePrefab;

    private ThreeCirclesDamageZone threeCirclesDamageZone;

    private List<Projectile> projectiles = new List<Projectile>();

    public void Setup()
    {
        threeCirclesDamageZone = GetComponent<ThreeCirclesDamageZone>();

        Sequence.Create()
            .ChainCallback(() => SetupCircle(0, PlayerStateMachine.instance.position))
            .ChainDelay(0.5f)
            .ChainCallback(() => SetupCircle(1, PlayerStateMachine.instance.position))
            .ChainDelay(0.5f)
            .ChainCallback(() => SetupCircle(2, PlayerStateMachine.instance.position))
            .ChainDelay(0.5f)
            .ChainCallback(() => Detonate());
    }

    private void SetupCircle(int index, Vector3 position)
    {
        threeCirclesDamageZone.Setup(index);
        threeCirclesDamageZone.MoveCircle(index, position.ToVector2());

        Vector3 hoverPosition = ComputeRockHoverPosition(position);

        Projectile projectile = Instantiate(projectilePrefab, hoverPosition + Vector3.up * 5.0f, Quaternion.identity);
        projectile.MoveToStartingPosition(hoverPosition, 0.3f);

        projectiles.Add(projectile);
    }

    private void Detonate()
    {
        threeCirclesDamageZone.Detonate();

        foreach (Projectile projectile in projectiles)
        {
            projectile.Shoot(ComputeRockTargetPosition(projectile.transform.position), 0.3f);
        }
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
