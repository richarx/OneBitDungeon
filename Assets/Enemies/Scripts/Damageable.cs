using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Enemies.Scripts
{
    public class Damageable : MonoBehaviour
    {
        [SerializeField] private int startingHealth;
        [SerializeField] private bool isInvincible;

        [HideInInspector] public UnityEvent<Vector2> OnTakeDamage = new UnityEvent<Vector2>();
        [HideInInspector] public UnityEvent OnDie = new UnityEvent();

        public int currentHealth { get; private set; }
        public bool IsDead => currentHealth <= 0;
        public bool IsFullLife => currentHealth == startingHealth;

        private void Start()
        {
            currentHealth = startingHealth;
        }

        public void TakeDamage(int damage, Vector2 direction)
        {
            if (IsDead)
                return;

            if (!isInvincible)
                currentHealth -= damage;

            if (IsDead)
                OnDie?.Invoke();
            else
                OnTakeDamage?.Invoke(direction);
        }

        [Button]
        public void InstantKill()
        {
            TakeDamage(currentHealth, Vector2.left);
        }
    }
}
