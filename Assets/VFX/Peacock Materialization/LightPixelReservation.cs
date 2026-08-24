using System;

public readonly struct LightPixelReservation
{
    public readonly LightPixelSwarm.ParticleData[] Particles;

    public bool IsValid => Particles != null;
    public int Count => Particles?.Length ?? 0;

    public LightPixelReservation(LightPixelSwarm.ParticleData[] particles)
    {
        Particles = particles ?? Array.Empty<LightPixelSwarm.ParticleData>();
    }
}

