using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class AfterImage : MonoBehaviour
{
    [SerializeField] private SpriteRenderer target;
    [SerializeField] private AfterImageFader afterImagePrefab;

    [Space]
    [SerializeField] private float frequency;
    [SerializeField] private float fadeDuration;

    [HideInInspector] public UnityEvent OnTriggerCancel = new UnityEvent();

    private bool isCancel;

    public void Trigger(float duration)
    {
        StopAllCoroutines();
        StartCoroutine(TriggerCoroutine(duration));
    }

    public void Cancel()
    {
        isCancel = true;
        StopAllCoroutines();
        OnTriggerCancel?.Invoke();
    }

    private IEnumerator TriggerCoroutine(float duration)
    {
        float timer = 0.0f;
        float increment = 1.0f / frequency;

        while (timer <= duration && !isCancel)
        {
            SpawnAfterImage();
            yield return new WaitForSeconds(increment);
            timer += increment;
        }
    }

    private void SpawnAfterImage()
    {
        Instantiate(afterImagePrefab, target.transform.position, target.transform.rotation)
            .Setup(this, target.sprite, fadeDuration);
    }
}
