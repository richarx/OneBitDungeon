using SFX;
using UnityEngine;

namespace Enemies.Goon
{
    public class GoonSfx : MonoBehaviour
    {
        [SerializeField] private AudioClip deathSound;
        [SerializeField] private AudioClip hitSound;
        [SerializeField] private AudioClip attackAnticipation_1;
        [SerializeField] private AudioClip attackAnticipation_2;
        [SerializeField] private AudioClip attackStart;
        
        private GoonStateMachine goon;

        private void Start()
        {
            goon = GetComponent<GoonStateMachine>();
            
            goon.damageable.OnTakeDamage.AddListener(() => SFXManager.instance.PlaySFX(hitSound));
            goon.damageable.OnDie.AddListener(() => SFXManager.instance.PlaySFX(deathSound));
            goon.goonSwordAttack.OnGoonSwordAttackAnticipation.AddListener(() =>
            {
                SFXManager.instance.PlaySFX(attackAnticipation_1, 0.03f);
                SFXManager.instance.PlaySFX(attackAnticipation_2);
            });
            goon.goonSwordAttack.OnGoonSwordAttack.AddListener((_,_) => SFXManager.instance.PlaySFX(attackStart, 0.05f));
        }
    }
}
