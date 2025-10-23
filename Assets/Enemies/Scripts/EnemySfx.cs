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
        [SerializeField] private AudioClip attackAnticipation_1;
        [SerializeField] private AudioClip attackAnticipation_2;
        [SerializeField] private AudioClip attackStart;
        
        private EnemyStateMachine _enemy;

        private void Start()
        {
            _enemy = GetComponent<EnemyStateMachine>();
            
            _enemy.damageable.OnTakeDamage.AddListener(() => SFXManager.instance.PlayRandomSFX(hitSound));
            _enemy.damageable.OnDie.AddListener(() => SFXManager.instance.PlayRandomSFX(deathSound));
            
            /*
            _enemy.goonSwordAttack.OnGoonSwordAttackAnticipation.AddListener(() =>
            {
                SFXManager.instance.PlaySFX(attackAnticipation_1, 0.03f);
                SFXManager.instance.PlaySFX(attackAnticipation_2);
            });
            _enemy.goonSwordAttack.OnGoonSwordAttack.AddListener((_,_) => SFXManager.instance.PlaySFX(attackStart, 0.05f));
            */
        }
    }
}
