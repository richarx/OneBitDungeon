using System.Collections.Generic;
using Enemies;
using UnityEngine;
using UnityEngine.Events;

namespace Player.Sword_Hitboxes
{
    public class WeaponDamageTrigger : MonoBehaviour
    {
        public static UnityEvent<Vector3> OnHitEnemy = new UnityEvent<Vector3>();

        private List<Damageable> targetsHit = new List<Damageable>();
        
        private void OnTriggerEnter(Collider other)
        {
            Debug.Log($"On Trigger Enter : {other.gameObject.name}");
            
            Damageable damageable = other.GetComponent<Damageable>();

            if (damageable != null && !damageable.IsDead && !targetsHit.Contains(damageable))
            {
                damageable.TakeDamage(1);
                targetsHit.Add(damageable);
                
                if (damageable.CompareTag("Enemy"))
                    OnHitEnemy?.Invoke(damageable.transform.position);
            }
        }
    }
}
