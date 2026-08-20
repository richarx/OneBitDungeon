using System.Collections;
using Player.Scripts;
using Tools_and_Scripts;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ChromaticAberrationController : MonoBehaviour
{
    [SerializeField] private float durationIn;
    [SerializeField] private float durationOut;
    [SerializeField] private float intensity;

    private VolumeProfile profile;
    private ChromaticAberration chromaticAberration;

    private float startingValue;

    private void Start()
    {
        profile = GetComponent<Volume>().profile;
        profile.TryGet(out chromaticAberration);
        startingValue = chromaticAberration.intensity.value;

        PlayerStateMachine.instance.playerHealth.OnPlayerTakeDamage.AddListener((_) =>
        {
            StopAllCoroutines();
            StartCoroutine(PunchChromaticAbberation(durationIn, durationOut, intensity));
        });
    }

    private IEnumerator PunchChromaticAbberation(float durationIn, float durationOut, float targetIntensity)
    {
        float timer = 0.0f;
        while (timer <= durationIn)
        {
            chromaticAberration.intensity.value = Tools.NormalizeValueInRange(timer, 0.0f, durationIn, startingValue, targetIntensity);
            yield return null;
            timer += Time.deltaTime;
        }

        timer = durationOut;
        while (timer >= 0.0f)
        {
            chromaticAberration.intensity.value = Tools.NormalizeValueInRange(timer, 0.0f, durationOut, startingValue, targetIntensity);
            yield return null;
            timer -= Time.deltaTime;
        }
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
        chromaticAberration.intensity.value = startingValue;
    }
}
