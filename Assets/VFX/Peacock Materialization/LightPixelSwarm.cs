using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public sealed class LightPixelSwarm : MonoBehaviour
{
    [Serializable]
    public struct ParticleData
    {
        public Vector3 worldPosition;
        public Color color;
        public float size;
    }

    [SerializeField, Min(1)] private int bufferSize = 128;

    private ParticleSystem _particleSystem;
    private ParticleSystem.Particle[] _particles;

    private void Awake()
    {
        _particleSystem = GetComponent<ParticleSystem>();
    }

    public ParticleData[] TakeNearest(Vector3 worldTarget, int amount)
    {
        if (amount <= 0 || !isActiveAndEnabled)
            return Array.Empty<ParticleData>();

        if (_particleSystem == null)
            _particleSystem = GetComponent<ParticleSystem>();

        if (_particleSystem == null)
            return Array.Empty<ParticleData>();


        if (_particles == null || _particles.Length < _particleSystem.main.maxParticles)
            _particles = new ParticleSystem.Particle[_particleSystem.main.maxParticles];

        int count = _particleSystem.GetParticles(_particles);
        if (count == 0)
            return Array.Empty<ParticleData>();

        var candidates = new List<int>(count);
        for (int i = 0; i < count; i++)
        {
            if (_particles[i].remainingLifetime > 0f)
                candidates.Add(i);
        }

        candidates.Sort((left, right) =>
            (
                            (_particles[left].position - worldTarget).sqrMagnitude)
                .CompareTo((_particles[right].position - worldTarget).sqrMagnitude)
            );

        int takenCount = Mathf.Min(amount, candidates.Count);
        var result = new ParticleData[takenCount];
        for (int i = 0; i < takenCount; i++)
        {
            int index = candidates[i];
            ParticleSystem.Particle particle = _particles[index];
            result[i] = new ParticleData
            {
                worldPosition = particle.position,
                color = particle.GetCurrentColor(_particleSystem),
                size = particle.GetCurrentSize(_particleSystem)
            };

            // Remove the particle from the swarm so it doesn't get rendered anymore.
            particle.remainingLifetime = 0f;
            _particles[index] = particle;
        }

        // Update the particle system with the modified particles.
        _particleSystem.SetParticles(_particles, count);
        return result;
    }

    public void AddParticles(ParticleData[] particlesToAdd, float lifetime)
    {
        if (particlesToAdd == null || particlesToAdd.Length == 0 || !isActiveAndEnabled)
            return;

        if (_particleSystem == null)
            _particleSystem = GetComponent<ParticleSystem>();

        if (_particleSystem == null)
            return;

        if (_particles == null || _particles.Length < _particleSystem.main.maxParticles)
            _particles = new ParticleSystem.Particle[_particleSystem.main.maxParticles];

        int currentCount = _particleSystem.GetParticles(_particles);
        int maxParticles = _particleSystem.main.maxParticles;
        int amountToAdd = Mathf.Min(particlesToAdd.Length, maxParticles);
        float safeLifetime = Mathf.Max(0.01f, lifetime);

        // A constantly emitting swarm is usually already full. Replace its last particles when
        // necessary so returned pixels are never silently discarded at the end of an explosion.
        int insertionIndex = Mathf.Min(currentCount, maxParticles - amountToAdd);

        for (int i = 0; i < amountToAdd; i++)
        {
            ParticleData source = particlesToAdd[i];
            _particles[insertionIndex + i] = new ParticleSystem.Particle
            {
                position = source.worldPosition,
                startColor = source.color,
                startSize = source.size,
                remainingLifetime = safeLifetime,
                startLifetime = safeLifetime
            };
        }

        _particleSystem.SetParticles(_particles, Mathf.Max(currentCount, insertionIndex + amountToAdd));
    }
}
