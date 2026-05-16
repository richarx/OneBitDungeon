using System.Collections;
using Game_Manager;
using Tools_and_Scripts;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CameraLensDistortion : MonoBehaviour
{
    [SerializeField] private float intensity;

    private VolumeProfile profile;
    private LensDistortion lensDistortion;

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
        while (timer <= 0.5f)
        {
            lensDistortion.intensity.value = Tools.NormalizeValueInRange(timer, 0.0f, 0.5f, 0.0f, intensity);

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
