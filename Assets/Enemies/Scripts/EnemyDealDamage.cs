using Player.Scripts;
using UnityEngine;
using UnityEngine.Events;

namespace Enemies.Scripts
{
    public class EnemyDealDamage : MonoBehaviour
    {
        [SerializeField] private int damage;
        
        [HideInInspector] public UnityEvent<Vector3> OnHitPlayer = new UnityEvent<Vector3>();
        [HideInInspector] public UnityEvent OnHitParry = new UnityEvent();

        private bool hasHitPlayer;
        
        private void OnTriggerEnter(Collider other)
        {
            if (hasHitPlayer)
                return;
            
            Debug.Log($"On Enemy Trigger Enter : {other.gameObject.name}");
            
            PlayerHealth player = other.GetComponent<PlayerHealth>();

            if (player != null && !player.IsDead)
            {
                hasHitPlayer = true;
                
                Vector3 playerPosition = player.transform.position;
                Vector3 direction = ComputeHitDirection(playerPosition);

                if (player.IsParrying())
                {
                    player.TriggerParry();
                    OnHitParry?.Invoke();
                }
                else
                {
                    player.TakeDamage(damage, direction);
                    OnHitPlayer?.Invoke(playerPosition);
                }
            }
        }

        private Vector3 ComputeHitDirection(Vector3 playerPosition)
        {
            Vector3 direction = (playerPosition - transform.position);
            direction.y = 0.0f;
            return direction.normalized;
        }
    }
}
