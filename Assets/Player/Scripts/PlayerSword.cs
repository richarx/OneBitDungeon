using System;
using Player.Sword_Hitboxes;
using UnityEngine;
using UnityEngine.Events;
using static Player.Sword_Hitboxes.WeaponAnimationTriggers;

namespace Player.Scripts
{
    public class PlayerSword : MonoBehaviour
    {
        [SerializeField] private bool hasSword;
        
        [Space]
        [SerializeField] private GameObject hitboxPrefab;
        public WeaponAnimationTriggers weaponAnimationTriggers;
        
        [HideInInspector] public UnityEvent OnEquipSword = new UnityEvent();
        [HideInInspector] public UnityEvent OnSheatheSword = new UnityEvent();

        private PlayerStateMachine player;
        
        private bool currentlyHasSword;
        public bool CurrentlyHasSword => currentlyHasSword;
        private bool isSwordInHand;
        public bool IsSwordInHand => isSwordInHand;

        private GameObject currentHitbox;
    
        private void Start()
        {
            currentlyHasSword = hasSword;
            player = PlayerStateMachine.instance;
            player.playerAttack.OnPlayerAttack.AddListener((_) => isSwordInHand = true);
            player.playerParry.OnParry.AddListener(() => isSwordInHand = true);
            weaponAnimationTriggers.OnSpawnHitbox.AddListener(SpawnHitbox);
            weaponAnimationTriggers.OnRemoveHitbox.AddListener(RemoveHitbox);
        }

        private void SpawnHitbox(AttackDirection direction)
        {
            if (currentHitbox != null)
                RemoveHitbox();

            currentHitbox = Instantiate(hitboxPrefab, player.position, Quaternion.identity, transform);
            currentHitbox.transform.RotateAround(player.position, Vector3.up, ComputeHitboxDirection(direction));
        }

        private float ComputeHitboxDirection(AttackDirection direction)
        {
            switch (direction)
            {
                case AttackDirection.L:
                    return 180.0f;
                case AttackDirection.F:
                    return 90.0f;
                case AttackDirection.R:
                    return 0.0f;
                case AttackDirection.BR:
                    return -45.0f;
                case AttackDirection.B:
                    return -100.0f;
                case AttackDirection.BL:
                    return -145.0f;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        public void RemoveHitbox()
        {
            if (currentHitbox != null)
            {
                Destroy(currentHitbox);
                currentHitbox = null;
            }
        }

        private void LateUpdate()
        {
            if (hasSword != currentlyHasSword)
            {
                currentlyHasSword = hasSword;
                isSwordInHand = false;
            }
        }

        public void SwapSword()
        {
            if (currentlyHasSword)
            {
                isSwordInHand = !isSwordInHand;
                
                if (isSwordInHand)
                    OnEquipSword?.Invoke();
                else
                    OnSheatheSword?.Invoke();
            }
        }
    }
}
