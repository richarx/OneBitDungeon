using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

public class AfterImage : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer target;

    [SerializeField, Required, AssetsOnly]
    private AfterImageFader afterImagePrefab;

    [Space]
    [SerializeField] private float frequency;
    [SerializeField] private float fadeDuration;

    [HideInInspector] public UnityEvent OnTriggerCancel = new UnityEvent();

    private bool isCancel;

    public float Frequency => frequency;

    public void SetTarget(SpriteRenderer newTarget)
    {
        target = newTarget;
    }

    public void SpawnSnapshot()
    {
        if (target == null || target.sprite == null || afterImagePrefab == null)
            return;

        SpawnAfterImage();
    }

    public void Trigger(SpriteRenderer newTarget, float duration)
    {
        target = newTarget;
        Trigger(duration);
    }

    public void Trigger(float duration)
    {
        StopAllCoroutines();
        isCancel = false;

        if (target == null || afterImagePrefab == null || frequency <= 0.0f)
            return;

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
        if (target == null || target.sprite == null || afterImagePrefab == null)
            return;
        Instantiate(afterImagePrefab, target.transform.position, target.transform.rotation)
            .Setup(this, target, fadeDuration);
    }

}
