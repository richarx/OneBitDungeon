using UnityEngine;
using UnityEngine.Events;

namespace Enemies
{
    public class Damageable : MonoBehaviour
    {
        [SerializeField] private int startingHealth;

        [HideInInspector] public UnityEvent OnTakeDamage = new UnityEvent();
        [HideInInspector] public UnityEvent OnDie = new UnityEvent();
        
        private int currentHealth;
        public bool IsDead => currentHealth <= 0;
        public bool IsFullLife => currentHealth == startingHealth;

        private void Start()
        {
            currentHealth = startingHealth;
        }

        public void TakeDamage(int damage)
        {
            if (IsDead)
                return;

            currentHealth -= damage;
            
            if (IsDead)
                OnDie?.Invoke();
            else
                OnTakeDamage?.Invoke();
        }
    }
}
