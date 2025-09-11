using UnityEngine;
using UnityEngine.Events;

namespace Player.Sword_Hitboxes
{
    public class WeaponAnimationTriggers : MonoBehaviour
    {
        public enum AttackDirection
        {
            L,
            F,
            R,
            BR,
            B,
            BL
        }

        [HideInInspector] public UnityEvent<AttackDirection> OnSpawnHitbox = new UnityEvent<AttackDirection>();
        [HideInInspector] public UnityEvent OnRemoveHitbox = new UnityEvent();
        [HideInInspector] public UnityEvent OnAttackCanBeCanceled = new UnityEvent();
        
        public void SpawnHitbox(AttackDirection direction)
        {
            OnSpawnHitbox?.Invoke(direction);
        }
        
        public void RemoveHitbox()
        {
            OnRemoveHitbox?.Invoke();
        }

        public void CanBeCanceled()
        {
            OnAttackCanBeCanceled?.Invoke();
        }
    }
}
