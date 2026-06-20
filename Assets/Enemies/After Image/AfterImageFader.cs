using PrimeTween;
using UnityEngine;
using UnityEngine.Events;

public class AfterImageFader : MonoBehaviour
{
    public void Setup(AfterImage spawner, SpriteRenderer source, float fadeDuration)
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null || source == null || source.sprite == null)
        {
            Destroy(gameObject);
            return;
        }

        spriteRenderer.sprite = source.sprite;
        spriteRenderer.flipX = source.flipX;
        spriteRenderer.flipY = source.flipY;
        transform.localScale = source.transform.lossyScale;

        UnityAction cancelAction = null;
        Sequence sequence = Sequence.Create(useUnscaledTime: true)
            .Chain(Tween.Alpha(spriteRenderer, 0.0f, fadeDuration))
            .ChainCallback(() =>
            {
                if (spawner != null)
                    spawner.OnTriggerCancel.RemoveListener(cancelAction);

                Destroy(gameObject);
            });

        cancelAction = () =>
        {
            if (sequence.isAlive)
                sequence.Stop();

            if (spawner != null)
                spawner.OnTriggerCancel.RemoveListener(cancelAction);

            Destroy(gameObject);
        };

        if (spawner != null)
            spawner.OnTriggerCancel.AddListener(cancelAction);
    }
}
