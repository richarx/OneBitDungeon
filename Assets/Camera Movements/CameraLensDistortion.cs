using System.Collections;
using Game_Manager;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CameraLensDistortion : MonoBehaviour
{
    [SerializeField] private float targetIntensity;
    [SerializeField] private float transitionDuration;

    private VolumeProfile profile;
    private LensDistortion lensDistortion;
    private float velocity;

    private void Start()
    {
        profile = GetComponent<Volume>().profile;
        profile.TryGet(out lensDistortion);

        GameManager.OnPrepareToLeaveRoom.AddListener((d) =>
        {
            StopAllCoroutines();
            StartCoroutine(TriggerTransition());
        });
    }

    private IEnumerator TriggerTransition()
    {
        float timer = 0.0f;
        while (timer <= transitionDuration)
        {
            lensDistortion.intensity.value = Mathf.SmoothDamp(lensDistortion.intensity.value, targetIntensity, ref velocity, transitionDuration);

            yield return null;
            timer += Time.deltaTime;
        }

        lensDistortion.intensity.value = 0.0f;
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
        lensDistortion.intensity.value = 0.0f;
    }
}
