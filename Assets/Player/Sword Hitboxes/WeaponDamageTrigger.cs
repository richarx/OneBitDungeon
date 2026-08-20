using System.Collections.Generic;
using Enemies.Scripts;
using Player.Scripts;
using Tools_and_Scripts;
using UnityEngine;
using UnityEngine.Events;

namespace Player.Sword_Hitboxes
{
    public class WeaponDamageTrigger : MonoBehaviour
    {
        public static UnityEvent<AttackPayload, Vector3> OnHitEnemy = new UnityEvent<AttackPayload, Vector3>();

        private List<Damageable> targetsHit = new List<Damageable>();

        private AttackPayload attackPayload;

        public void SetPayload(AttackPayload payload)
        {
            attackPayload = payload;
        }

        private void OnTriggerEnter(Collider other)
        {
            Damageable damageable = other.GetComponent<Damageable>();

            if (damageable != null && !damageable.IsDead && !targetsHit.Contains(damageable))
            {
                Vector2 direction = (damageable.transform.position - PlayerStateMachine.instance.position).normalized.ToVector2();
                damageable.TakeDamage(attackPayload.damage, direction);
                targetsHit.Add(damageable);

                if (damageable.CompareTag("Enemy"))
                    OnHitEnemy?.Invoke(attackPayload, damageable.transform.position);
            }
        }
    }
}
