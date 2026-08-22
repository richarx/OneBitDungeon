using System;
using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using static LightPixelSwarm;


public sealed class PeacockKnightMaterialization : MonoBehaviour
{
    [TitleGroup("References")]
    [SerializeField]
    private LightPixelSwarm sourceSwarm;

    [TitleGroup("References")]
    [SerializeField]
    private ParticleSystem particleSystemForConverging;

    [TitleGroup("References")]
    [SerializeField]
    private SpriteRenderer luminousSilhouette;


    [TitleGroup("References")]
    [SerializeField]
    private GameObject knight;



    [TitleGroup("Particles selection")]
    [SerializeField, Range(1, 100)] private int particlesToBorrow = 30;

    [TitleGroup("Particles selection")]
    [SerializeField, Range(0, 255)] private int swarmAlpha = 10;

    [TitleGroup("Convergence")]
    [SerializeField, Min(0.05f)] private float convergenceDuration = 0.8f;
    [SerializeField, Range(0f, 1f)] private float maximumStartDelay = 0.18f;
    [SerializeField, Min(0f)] private float swirlDistance = 0.35f;
    [SerializeField, Min(0f)] private float swirlTurns = 0.6f;
    [SerializeField] private bool useUnscaledTime;

    [TitleGroup("Luminous Silhouette")]
    [SerializeField, Range(0f, 1f)] private float silhouetteStartAppearance = 0.66f;

    [TitleGroup("Luminous Silhouette")]
    [SerializeField, Min(0.01f)] private float silhouettePopDuration = 0.15f;



    private ParticleState[] _states = Array.Empty<ParticleState>();
    private ParticleSystem.Particle[] _renderParticles = Array.Empty<ParticleSystem.Particle>();
    private float _elapsed;
    private bool _isMaterializing;
    private bool _hasStartedSilhouettePop;
    private Vector3 _silhouetteBaseScale;
    private Color _silhouetteBaseColor;
    private Sequence _silhouettePopSequence;

    private struct ParticleState
    {
        public Vector3 start;
        public Color color;
        public float size;
        public float delay;
        public float phase;
    }

    private void Awake()
    {
        if (luminousSilhouette != null)
        {
            _silhouetteBaseScale = luminousSilhouette.transform.localScale;
            _silhouetteBaseColor = luminousSilhouette.color;
            luminousSilhouette.enabled = false;
        }
    }

    private void OnDisable()
    {
        CancelVisuals();
        if (knight != null)
            knight.SetActive(true);
    }

    [Button("Materialize")]
    public void Materialize()
    {
        if (!Application.isPlaying)
            return;

        if (knight == null || sourceSwarm == null || particleSystemForConverging == null)
        {
            Debug.LogWarning("Cannot materialize: missing references.");

            return;
        }

        CancelVisuals();
        knight.SetActive(false);

        Vector3 target = TargetPosition;
        ParticleData[] snapshots = sourceSwarm.TakeNearest(target, particlesToBorrow);

        _states = new ParticleState[snapshots.Length];
        _renderParticles = new ParticleSystem.Particle[snapshots.Length];

        for (int i = 0; i < snapshots.Length; i++)
        {
            _states[i] = new ParticleState
            {
                start = snapshots[i].worldPosition,
                color = snapshots[i].color,
                size = snapshots[i].size,

                delay = UnityEngine.Random.Range(0f, maximumStartDelay * convergenceDuration),
                phase = UnityEngine.Random.value * Mathf.PI * 2f
            };
        }

        _elapsed = 0f;
        _isMaterializing = true;
        if (particleSystemForConverging != null)
        {
            particleSystemForConverging.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            particleSystemForConverging.Play(true);
            // Populate immediately so borrowed particles do not disappear for one frame.
            UpdateConvergingParticles();
        }
    }

    [Button("Materialize Randomly")]
    public void MaterializeRandomly()
    {
        this.transform.position = new Vector3(
            UnityEngine.Random.Range(-7f, 7f),
            transform.position.y,
            UnityEngine.Random.Range(-7f, 7f)
        );
        Materialize();
    }

    private void Update()
    {
        if (!_isMaterializing)
            return;

        _elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        float overallProgress = Mathf.Clamp01(_elapsed / convergenceDuration);
        UpdateConvergingParticles();
        TryStartSilhouettePop(overallProgress);

        if (overallProgress >= 1f && (!_hasStartedSilhouettePop || !_silhouettePopSequence.isAlive))
            CompleteMaterialization();
    }

    private void UpdateConvergingParticles()
    {
        if (particleSystemForConverging == null || _states.Length == 0)
            return;

        Vector3 target = TargetPosition;
        for (int i = 0; i < _states.Length; i++)
        {
            ParticleState state = _states[i];
            float durationAfterDelay = Mathf.Max(0.001f, convergenceDuration - state.delay);
            float progress = Mathf.Clamp01((_elapsed - state.delay) / durationAfterDelay);

            // ease in-out cubic: f(t) = 3t^2 - 2t^3
            float eased = progress * progress * (3f - 2f * progress);


            Vector3 position = Vector3.LerpUnclamped(state.start, target, eased);


            // On arrive en sinusoïde pour faire un effet de tourbillon vers la cible
            Vector3 direction = target - state.start;
            Vector3 perpendicular = new Vector3(-direction.y, direction.x, 0f).normalized;
            float wave = Mathf.Sin(progress * Mathf.PI) * swirlDistance;
            float angleWave = Mathf.Sin(progress * Mathf.PI * Mathf.Max(1f, swirlTurns * 2f) + state.phase);
            position += perpendicular * wave * angleWave;



            ParticleSystem.Particle particle = new ParticleSystem.Particle
            {
                position = position,
                startColor = ColorWithAlpha(state.color, Mathf.Lerp(SwarmAlpha, 1f, EaseOutQuad(progress))),
                startSize = state.size,
                remainingLifetime = 1f,
                startLifetime = 1f
            };

            _renderParticles[i] = particle;
        }

        particleSystemForConverging.SetParticles(_renderParticles, _renderParticles.Length);
    }

    private void TryStartSilhouettePop(float progress)
    {
        if (_hasStartedSilhouettePop || luminousSilhouette == null || progress < silhouetteStartAppearance)
            return;

        _hasStartedSilhouettePop = true;

        Vector3 collapsedScale = _silhouetteBaseScale;
        collapsedScale.x = 0f;
        luminousSilhouette.transform.localScale = collapsedScale;

        Color transparentColor = _silhouetteBaseColor;
        transparentColor.a = 0f;
        luminousSilhouette.color = transparentColor;
        luminousSilhouette.enabled = true;

        _silhouettePopSequence = Sequence.Create(useUnscaledTime: useUnscaledTime)
            .Group(Tween.ScaleX(luminousSilhouette.transform, _silhouetteBaseScale.x, silhouettePopDuration, Ease.OutQuad))
            .Group(Tween.Alpha(luminousSilhouette, _silhouetteBaseColor.a, silhouettePopDuration, Ease.OutQuad));
    }

    private void CompleteMaterialization()
    {
        _isMaterializing = false;
        ClearConvergingPixels();
        if (luminousSilhouette != null)
            luminousSilhouette.enabled = false;
        if (knight != null && knight != gameObject)
        {
            knight.transform.position = LuminousSilhouettePosition;
            knight.SetActive(true);
        }
    }

    private void CancelVisuals()
    {
        _isMaterializing = false;
        _elapsed = 0f;
        _hasStartedSilhouettePop = false;
        if (_silhouettePopSequence.isAlive)
            _silhouettePopSequence.Stop();
        ClearConvergingPixels();
        if (luminousSilhouette != null)
        {
            luminousSilhouette.enabled = false;
            luminousSilhouette.transform.localScale = _silhouetteBaseScale;
            luminousSilhouette.color = _silhouetteBaseColor;
        }
    }

    private void ClearConvergingPixels()
    {
        if (particleSystemForConverging != null)
            particleSystemForConverging.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private Vector3 TargetPosition => transform.position;

    // The normal knight must replace the glowing silhouette at the exact same point.
    private Vector3 LuminousSilhouettePosition => luminousSilhouette != null
        ? luminousSilhouette.transform.position
        : TargetPosition;

    private float SwarmAlpha => swarmAlpha / 255f;

    private static Color ColorWithAlpha(Color color, float alpha)
    {
        color.a = alpha;
        return color;
    }

    // QuadOut: fast brightening at the start, then a soft arrival at full light.
    private static float EaseOutQuad(float progress)
    {
        progress = Mathf.Clamp01(progress);
        return 1f - (1f - progress) * (1f - progress);
    }

}
