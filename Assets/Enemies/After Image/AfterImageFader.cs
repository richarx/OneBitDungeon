using PrimeTween;
using UnityEngine;

public class AfterImageFader : MonoBehaviour
{
    public void Setup(AfterImage spawner, Sprite sprite, float fadeDuration)
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;

        Sequence sequence = Sequence.Create()
            .Chain(Tween.Alpha(spriteRenderer, 0.0f, fadeDuration))
            .ChainCallback(() => Destroy(gameObject));

        spawner.OnTriggerCancel.AddListener(() =>
        {
            if (sequence.isAlive)
            {
                sequence.Stop();
                Destroy(gameObject);
            }
        });
    }
}
