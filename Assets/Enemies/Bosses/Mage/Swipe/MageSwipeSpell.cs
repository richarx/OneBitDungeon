using PrimeTween;
using Tools_and_Scripts;
using UnityEngine;

public class MageSwipeSpell : MonoBehaviour
{
    [SerializeField] private Projectile projectilePrefab;

    private Sequence sequence;
    private RectangleDamageZone rectangleDamageZone;
    private Projectile projectile;

    public void Setup(Vector2 direction, float spawnDuration, float fillDuration)
    {
        rectangleDamageZone = GetComponent<RectangleDamageZone>();
        rectangleDamageZone.Setup(direction, spawnDuration, fillDuration);

        Vector3 hoverPosition = transform.position;
        Vector3 spawnPosition = hoverPosition + direction.ToVector3() * -2.0f;
        Vector3 targetPosition = hoverPosition + direction.ToVector3() * 24.0f;

        projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
        projectile.MoveToStartingPosition(hoverPosition, 0.3f);

        sequence = Sequence.Create()
            .ChainDelay(spawnDuration)
            .ChainDelay(fillDuration)
            .ChainCallback(() => projectile.Shoot(targetPosition, 0.5f));
    }

    public void Cancel()
    {
        if (rectangleDamageZone != null)
            rectangleDamageZone.Cancel();

        if (sequence.isAlive)
            sequence.Stop();

        if (projectile != null)
            projectile.CancelProjectile();
    }
}
