using System.Collections.Generic;
using Enemies;
using UnityEngine;

namespace Player.Sword_Hitboxes
{
    public class WeaponDamageTrigger : MonoBehaviour
    {
        private List<Damageable> targetsHit = new List<Damageable>();
        
        private void OnTriggerEnter(Collider other)
        {
            Debug.Log($"On Trigger Enter : {other.gameObject.name}");
            
            Damageable damageable = other.GetComponent<Damageable>();

            if (damageable != null && !damageable.IsDead && !targetsHit.Contains(damageable))
            {
                damageable.TakeDamage(1);
                targetsHit.Add(damageable);
            }
        }
    }
}
