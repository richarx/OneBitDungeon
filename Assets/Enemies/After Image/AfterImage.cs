using System.Collections;
using PrimeTween;
using UnityEngine;

public class AfterImage : MonoBehaviour
{
    [SerializeField] private SpriteRenderer target;
    [SerializeField] private SpriteRenderer afterImagePrefab;

    [Space]
    [SerializeField] private float frequency;
    [SerializeField] private float fadeDuration;

    public void Trigger(float duration)
    {
        StopAllCoroutines();
        StartCoroutine(TriggerCoroutine(duration));
    }

    private IEnumerator TriggerCoroutine(float duration)
    {
        float timer = 0.0f;
        float increment = 1.0f / frequency;

        while (timer <= duration)
        {
            SpawnAfterImage();
            yield return new WaitForSeconds(increment);
            timer += increment;
        }
    }

    private void SpawnAfterImage()
    {
        SpriteRenderer afterImage = Instantiate(afterImagePrefab, target.transform.position, target.transform.rotation);
        afterImage.sprite = target.sprite;

        Sequence.Create()
            .Chain(Tween.Alpha(afterImage, 0.0f, fadeDuration))
            .ChainCallback(() => Destroy(afterImage.gameObject));
    }
}
