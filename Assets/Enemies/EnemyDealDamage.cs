using Player.Scripts;
using UnityEngine;
using UnityEngine.Events;

namespace Enemies
{
    public class EnemyDealDamage : MonoBehaviour
    {
        [SerializeField] private int damage;
        
        [HideInInspector] public UnityEvent<Vector3> OnHitPlayer = new UnityEvent<Vector3>();

        private bool hasHitPlayer;
        
        private void OnTriggerEnter(Collider other)
        {
            Debug.Log($"On Enemy Trigger Enter : {other.gameObject.name}");
            
            PlayerHealth player = other.GetComponent<PlayerHealth>();

            if (player != null && !player.IsDead && !hasHitPlayer)
            {
                Vector3 playerPosition = player.transform.position;
                Vector3 direction = (playerPosition - transform.position);
                direction.y = 0.0f;
                
                player.TakeDamage(damage, direction.normalized);
                hasHitPlayer = true;
                OnHitPlayer?.Invoke(playerPosition);
            }
        }
    }
}
