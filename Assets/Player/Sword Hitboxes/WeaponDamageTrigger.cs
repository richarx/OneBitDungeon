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
        private bool _wasDeflected;

        public void SetPayload(AttackPayload payload)
        {
            attackPayload = payload;
            _wasDeflected = false;
            targetsHit.Clear();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_wasDeflected || !gameObject.activeInHierarchy)
                return;

            Damageable damageable = other.GetComponent<Damageable>();

            if (damageable != null && !damageable.IsDead && !targetsHit.Contains(damageable))
            {
                Vector2 direction = (damageable.transform.position - PlayerStateMachine.instance.position).normalized.ToVector2();
                bool hitWasApplied = damageable.TryTakeDamage(attackPayload, direction);
                targetsHit.Add(damageable);

                if (!hitWasApplied)
                {
                    _wasDeflected = true;
                    PlayerStateMachine player = PlayerStateMachine.instance;
                    player.playerDeflected.Trigger(player, damageable.transform.position);
                    return;
                }

                if (hitWasApplied && damageable.CompareTag("Enemy"))
                    OnHitEnemy?.Invoke(attackPayload, damageable.transform.position);
            }
        }
    }
}
