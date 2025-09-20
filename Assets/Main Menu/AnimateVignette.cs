using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Main_Menu
{
    public class AnimateVignette : MonoBehaviour
    {
        [SerializeField] private float intensity;
        [SerializeField] private float frequency;
        [SerializeField] private float amplitude;
        
        private Vignette vignette;

        private void Start()
        {
            GetComponent<Volume>().profile.TryGet<Vignette>(out vignette);
        }

        private void Update()
        {
            vignette.intensity.value = intensity + amplitude * Mathf.Sin(Time.time * frequency);
        }
    }
}
