using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using static LightPixelSwarm;

public sealed class PeacockKnightDematerialization : MonoBehaviour
{
    [TitleGroup("References")]
    [SerializeField] private LightPixelSwarm destinationSwarm;

    [TitleGroup("References")]
    [SerializeField] private ParticleSystem particleSystemForDispersion;

    [TitleGroup("References")]
    [SerializeField] private GameObject knight;

    [TitleGroup("Burst")]
    [SerializeField, Range(1, 100)] private int pixelsToBurst = 30;

    [TitleGroup("Burst")]
    [SerializeField, Min(0f)] private float spawnRadius = 0.1f;

    [TitleGroup("Burst")]
    [SerializeField, Min(0f)] private float minimumSpeed = 7f;

    [TitleGroup("Burst")]
    [SerializeField, Min(0f)] private float maximumSpeed = 12f;

    [TitleGroup("Burst")]
    [SerializeField, Min(0f)] private float minimumUpwardSpeed = 2f;

    [TitleGroup("Burst")]
    [SerializeField, Min(0f)] private float maximumUpwardSpeed = 5f;

    [TitleGroup("Burst")]
    [SerializeField, Min(0f)] private float gravity = 14f;

    [TitleGroup("Burst")]
    [SerializeField, Min(0f)] private float braking = 20f;

    [TitleGroup("Burst")]
    [SerializeField, Min(0.05f)] private float brakingDuration = 0.55f;

    [TitleGroup("Burst")]
    [SerializeField] private bool useUnscaledTime;

    [TitleGroup("Pixels")]
    [SerializeField, Min(0.01f)] private float pixelSize = 0.25f;

    [TitleGroup("Ambient Return")]
    [SerializeField, Min(0.01f)] private float returnedPixelLifetime = 20f;

    private BurstPixel[] _burstPixels = Array.Empty<BurstPixel>();
    private ParticleSystem.Particle[] _renderParticles = Array.Empty<ParticleSystem.Particle>();
    private float _elapsed;
    private bool _isDematerializing;

    private struct BurstPixel
    {
        public Vector3 position;
        public Vector3 velocity;
    }


    private void OnDisable()
    {
        if (_isDematerializing)
        {
            ReturnPixelsToSwarm();
            if (knight != null)
                knight.SetActive(true);
        }

        CancelBurst();
    }

    [Button("Dematerialize")]
    public void Dematerialize()
    {
        if (!Application.isPlaying)
            return;

        if (destinationSwarm == null || particleSystemForDispersion == null || knight == null || knight == gameObject)
        {
            Debug.LogWarning("Cannot dematerialize: missing references or the knight is the materializer object.", this);
            return;
        }

        if (_isDematerializing)
            ReturnPixelsToSwarm();

        CancelBurst();
        knight.SetActive(false);


        // flemme
        Vector3 origin = knight.transform.position;
        _burstPixels = new BurstPixel[pixelsToBurst];
        _renderParticles = new ParticleSystem.Particle[pixelsToBurst];


        for (int i = 0; i < _burstPixels.Length; i++)
        {
            Vector2 sideDirection = UnityEngine.Random.insideUnitCircle.normalized;

            Vector3 horizontalVelocity = new Vector3(sideDirection.x, 0f, sideDirection.y) * UnityEngine.Random.Range(minimumSpeed, maximumSpeed);
            Vector3 velocity = horizontalVelocity + Vector3.up * UnityEngine.Random.Range(minimumUpwardSpeed, maximumUpwardSpeed);
            _burstPixels[i] = new BurstPixel
            {
                position = origin + UnityEngine.Random.insideUnitSphere * spawnRadius,
                velocity = velocity
            };
        }



        _elapsed = 0f;
        _isDematerializing = true;
        particleSystemForDispersion.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        particleSystemForDispersion.Play(true);
        UpdateBurstParticles();
    }

    private void Update()
    {
        if (!_isDematerializing)
            return;

        float deltaTime = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        _elapsed += deltaTime;
        UpdateBurstParticles(deltaTime);

        if (_elapsed >= brakingDuration)
        {
            ReturnPixelsToSwarm();
            CancelBurst();
        }
    }

    private void UpdateBurstParticles(float deltaTime = 0f)
    {
        if (particleSystemForDispersion == null || _burstPixels.Length == 0)
            return;

        for (int i = 0; i < _burstPixels.Length; i++)
        {
            BurstPixel burstPixel = _burstPixels[i];
            burstPixel.velocity += Vector3.down * gravity * deltaTime;

            // Strongly brake the outward motion while keeping gravity free to pull the pixels back down.
            Vector3 horizontalVelocity = new Vector3(burstPixel.velocity.x, 0f, burstPixel.velocity.z);
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, Vector3.zero, braking * deltaTime);
            burstPixel.velocity.x = horizontalVelocity.x;
            burstPixel.velocity.z = horizontalVelocity.z;
            burstPixel.position += burstPixel.velocity * deltaTime;
            _burstPixels[i] = burstPixel;

            _renderParticles[i] = new ParticleSystem.Particle
            {
                position = burstPixel.position,
                startColor = Color.white,
                startSize = pixelSize,
                remainingLifetime = 1f,
                startLifetime = 1f
            };
        }

        particleSystemForDispersion.SetParticles(_renderParticles, _renderParticles.Length);
    }

    private void ReturnPixelsToSwarm()
    {
        if (destinationSwarm == null || _burstPixels.Length == 0)
            return;

        var returnedPixels = new ParticleData[_burstPixels.Length];
        for (int i = 0; i < _burstPixels.Length; i++)
        {
            BurstPixel burstPixel = _burstPixels[i];
            returnedPixels[i] = new ParticleData
            {
                worldPosition = burstPixel.position,
                color = Color.white,
                size = pixelSize
            };
        }

        destinationSwarm.AddParticles(returnedPixels, returnedPixelLifetime);
    }

    private void CancelBurst()
    {
        _isDematerializing = false;
        _elapsed = 0f;
        if (particleSystemForDispersion != null)
            particleSystemForDispersion.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

}
