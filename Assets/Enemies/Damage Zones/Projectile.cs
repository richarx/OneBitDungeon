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
    private bool isCanceled;
    private bool isDestroyed;

    public void CancelProjectile()
    {
        if (isCanceled)
            return;

        isCanceled = true;

        if (currentSequence.isAlive)
            currentSequence.Stop();

        DestroyProjectile();
    }

    public void MoveToStartingPosition(Vector3 targetPosition, float moveDuration)
    {
        if (isCanceled)
            return;

        PlaySfx(moveClip);

        if (currentSequence.isAlive)
            currentSequence.Stop();

        currentSequence = Sequence.Create()
            .Chain(Tween.LocalPosition(transform, targetPosition, moveDuration, Ease.OutBack));
    }

    public void Shoot(Vector3 targetPosition, float moveDuration)
    {
        if (isCanceled)
            return;

        PlaySfx(shootClip);

        Vector3 punchScale = new Vector3(1.3f, 0.7f, 1.0f);

        if (currentSequence.isAlive)
            currentSequence.Stop();

        currentSequence = Sequence.Create()
            .Chain(Tween.Position(transform, targetPosition, moveDuration, Ease.InOutBack))
            .Group(Tween.PunchScale(transform, punchScale, 0.1f, 3.0f, startDelay: 0.5f))
            .ChainCallback(() => SpawnImpact())
            .ChainCallback(() => DestroyProjectile());
    }

    private void SpawnImpact()
    {
        PlaySfx(impactClip);
        Instantiate(impactPrefab, transform.position, Quaternion.identity);
    }

    private void PlaySfx(AudioClip clip, float volume = 0.2f)
    {
        if (clip != null)
            SFXManager.instance.PlaySFXInChannel($"{clip.name}", 0.3f, clip);
    }

    private void DestroyProjectile()
    {
        if (isDestroyed)
            return;

        isDestroyed = true;
        Destroy(gameObject);
    }
}
