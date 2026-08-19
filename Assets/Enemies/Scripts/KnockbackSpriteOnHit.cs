using Enemies.Scripts;
using PrimeTween;
using Tools_and_Scripts;
using UnityEngine;

public class KnockbackSpriteOnHit : MonoBehaviour
{
    [SerializeField] private Transform spriteTarget;
    [SerializeField] private float strength;

    private Sequence currentSequence;

    private void OnEnable()
    {
        Damageable damageable = GetComponent<Damageable>();

        if (damageable != null)
            damageable.OnTakeDamage.AddListener(KnockbackSprite);
    }

    private void OnDisable()
    {
        Damageable damageable = GetComponent<Damageable>();

        if (damageable != null)
            damageable.OnTakeDamage.RemoveListener(KnockbackSprite);
    }

    private void KnockbackSprite(Vector2 direction)
    {
        if (currentSequence.isAlive)
            currentSequence.Stop();

        currentSequence = Sequence.Create()
            .Chain(Tween.PunchLocalPosition(spriteTarget, direction.ToVector3() * strength, 0.3f, easeBetweenShakes: Ease.OutBack));
    }
}
