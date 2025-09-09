using System.Collections;
using UnityEngine;

public class ShakeIn2D : MonoBehaviour
    {
        public Transform _Target;

        public Vector2 _Seed;
        public float _Speed = 20f;
        public float _MaxMagnitude = 0.3f;
        public float _NoiseMagnitude = 0.3f;

        public Vector2 _Direction = Vector2.up;

        public float duration;
        
        float _FadeOut = 1f;

        void Update()
        {
            var sin = Mathf.Sin(_Speed * (_Seed.x + _Seed.y + Time.time));
            var direction = _Direction + GetNoise();
            direction.Normalize();
            var delta = direction * sin;
            _Target.localPosition = delta * (Time.deltaTime * (_MaxMagnitude * _FadeOut));
        }

        public void FireOnce()
        {
            StopAllCoroutines();
            StartCoroutine(ShakeAndFade(duration));
        }

        public void ContinuousShake()
        {
            enabled = true;
            _FadeOut = 1f;
        }

        public IEnumerator ShakeAndFade(float fade_duration)
        {
            enabled = true;
            _FadeOut = 1f;
            var fade_out_start = Time.time;
            var fade_out_complete = fade_out_start + fade_duration;
            while (Time.time < fade_out_complete)
            {
                yield return null;
                var t = 1f - Mathf.InverseLerp(fade_out_start, fade_out_complete, Time.time);
                // Pass t into an easing function from
                // https://github.com/idbrii/cs-tween/blob/main/Easing.cs to
                // make it nonlinear. CircIn is nice. See them visualized at
                // https://easings.net/
                _FadeOut = t;
            }
            enabled = false;
        }

        Vector2 GetNoise()
        {
            var time = Time.time;
            return _NoiseMagnitude
                * new Vector2(
                    Mathf.PerlinNoise(_Seed.x, time) - 0.5f,
                    Mathf.PerlinNoise(_Seed.y, time) - 0.5f
                    );
        }
        
    }