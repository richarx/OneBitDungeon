using PrimeTween;
using SFX;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private AudioClip moveClip;
    [SerializeField] private AudioClip shootClip;
    [SerializeField] private AudioClip impactClip;
    [SerializeField] private GameObject impactPrefab;

    private Sequence currentSequence;

    public void CancelProjectile()
    {
        if (currentSequence.isAlive)
            currentSequence.Stop();

        DestroyProjectile();
    }

    public void MoveToStartingPosition(Vector3 targetPosition, float moveDuration)
    {
        if (moveClip != null)
            SFXManager.instance.PlaySFX(moveClip);

        if (currentSequence.isAlive)
            currentSequence.Stop();

        currentSequence = Sequence.Create()
            .Chain(Tween.LocalPosition(transform, targetPosition, moveDuration, Ease.OutBack));
    }

    public void Shoot(Vector3 targetPosition, float moveDuration)
    {
        if (shootClip != null)
            SFXManager.instance.PlaySFX(shootClip);

        Vector3 punchScale = new Vector3(1.3f, 0.7f, 1.0f);

        if (currentSequence.isAlive)
            currentSequence.Stop();

        currentSequence = Sequence.Create()
            .Chain(Tween.LocalPosition(transform, targetPosition, moveDuration, Ease.InOutBack))
            .Group(Tween.PunchScale(transform, punchScale, 0.1f, 3.0f, startDelay: 0.5f))
            .ChainCallback(() => SpawnImpact())
            .ChainCallback(() => DestroyProjectile());
    }

    private void SpawnImpact()
    {
        if (impactClip != null)
            SFXManager.instance.PlaySFX(impactClip);

        Instantiate(impactPrefab, transform.position, Quaternion.identity);
    }

    private void DestroyProjectile()
    {
        Destroy(gameObject);
    }
}
