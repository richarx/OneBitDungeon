using Player.Scripts;
using Sirenix.OdinInspector;
using Tools_and_Scripts;
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
        [HideInInspector] public UnityEvent OnResetHealth = new UnityEvent();

        public int currentHealth { get; private set; }
        public int maxHealth { get; private set; }
        public bool IsDead => currentHealth <= 0;
        public bool IsFullLife => currentHealth == startingHealth;
        public bool IsInvincible
        {
            get => isInvincible;
            set => isInvincible = value;
        }
        public float currentHealthNormalized => Tools.NormalizeValue(currentHealth, 0.0f, maxHealth);

        private void Awake()
        {
            maxHealth = startingHealth;
            currentHealth = startingHealth;
        }

        public void TakeDamage(AttackPayload attackPayload, Vector2 direction)
        {
            if (IsDead)
                return;

            if (!isInvincible)
            {
                if (attackPayload.Type == AttackType.Critical)
                    currentHealth = 0;
                else
                    currentHealth -= attackPayload.damage;
            }

            if (IsDead)
                OnDie?.Invoke();
            else
                OnTakeDamage?.Invoke(direction);
        }

        [Button]
        public void InstantKill()
        {
            TakeDamage(new AttackPayload("Instant Kill", AttackType.Light, currentHealth, 1), Vector2.left);
        }

        public void ResetHealth(int newHealthCount)
        {
            maxHealth = newHealthCount;
            currentHealth = newHealthCount;
            OnResetHealth?.Invoke();
        }
    }
}
