using System.Collections.Generic;
using Enemies.Scripts.Behaviours;
using SFX;
using UnityEngine;

namespace Enemies.Scripts
{
    public class EnemySfx : MonoBehaviour
    {
        [SerializeField] private List<AudioClip> deathSound;
        [SerializeField] private List<AudioClip> hitSound;

        private void Start()
        {
            Damageable damageable = GetComponent<Damageable>();

            damageable.OnTakeDamage.AddListener(() => SFXManager.instance.PlayRandomSFX(hitSound));
            damageable.OnDie.AddListener(() => SFXManager.instance.PlayRandomSFX(deathSound));
        }
    }
}
