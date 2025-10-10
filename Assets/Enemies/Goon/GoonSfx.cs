using SFX;
using UnityEngine;

namespace Enemies.Goon
{
    public class GoonSfx : MonoBehaviour
    {
        [SerializeField] private AudioClip deathSound;
        [SerializeField] private AudioClip hitSound;
        
        private GoonStateMachine goon;

        private void Start()
        {
            goon = GetComponent<GoonStateMachine>();
            
            goon.damageable.OnTakeDamage.AddListener(SpawnDamageSfx);
            goon.damageable.OnDie.AddListener(SpawnDeathSfx);
        }

        private void SpawnDamageSfx()
        {
            SFXManager.instance.PlaySFX(hitSound);
        }

        private void SpawnDeathSfx()
        {
            SFXManager.instance.PlaySFX(deathSound);
        }
    }
}
