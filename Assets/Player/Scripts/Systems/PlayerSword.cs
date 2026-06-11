using Tools_and_Scripts;
using UnityEngine;
using UnityEngine.Events;

namespace Player.Scripts
{
    public class PlayerSword : MonoBehaviour
    {
        [SerializeField] private bool hasSword;

        [Space]
        [SerializeField] private GameObject hitboxPrefab;
        [SerializeField] private GameObject specialHitboxPrefab;
        [SerializeField] private Vector3 whirlwindHitboxLocalPosition = new Vector3(0.0f, 0.75f, 0.0f);
        [SerializeField] private Vector3 whirlwindHitboxLocalScale = new Vector3(5.0f, 1.5f, 5.0f);

        [Space]
        [SerializeField] private GameObject jumpTagHitboxPrefab;
        [SerializeField] private Vector3 jumpTagHitboxLocalPosition = new Vector3(0.0f, 0.75f, 0.0f);
        [SerializeField] private Vector3 jumpTagHitboxLocalScale = new Vector3(4.0f, 1.5f, 4.0f);

        [HideInInspector] public UnityEvent OnEquipSword = new UnityEvent();
        [HideInInspector] public UnityEvent OnSheatheSword = new UnityEvent();

        private PlayerStateMachine player;

        private bool currentlyHasSword;
        public bool CurrentlyHasSword => currentlyHasSword;
        private bool isSwordInHand;
        public bool IsSwordInHand => isSwordInHand;

        private GameObject currentHitbox;
        private AttackPayload currentAttackPayload;

        private void Start()
        {
            currentlyHasSword = hasSword;
            player = PlayerStateMachine.instance;
            player.playerParry.OnStartParry.AddListener(() => isSwordInHand = true);
            player.playerSit.OnStartSittingDown.AddListener(() => isSwordInHand = false);

            player.playerAttack.OnPlayerAttack.AddListener(HandlePlayerAttack);
            player.playerAttack.OnSpawnDamageBox.AddListener(SpawnHitbox);
            player.playerAttack.OnRemoveDamageBox.AddListener(RemoveHitbox);

            player.playerJumpTag.OnStartJumpTag.AddListener(HandleJumpTagStart);
            player.playerJumpTag.OnSpawnDamageBox.AddListener(SpawnJumpTagHitbox);
            player.playerJumpTag.OnRemoveDamageBox.AddListener(RemoveHitbox);
        }

        private void HandlePlayerAttack(AttackPayload attackPayload)
        {
            currentAttackPayload = attackPayload;
            isSwordInHand = true;
        }

        private void HandleJumpTagStart()
        {
            currentAttackPayload = null;
            ForceSwordInHand();
        }

        private void SpawnHitbox()
        {
            if (currentHitbox != null)
                RemoveHitbox();


            // Plus tard ajouter le knockback dans l'attack payload
            if (currentAttackPayload != null && (currentAttackPayload.Type == AttackType.Special || currentAttackPayload.Type == AttackType.Punish))
                SpawnWhirlwindHitbox();
            else
                SpawnDirectionalHitbox();
        }

        private void SpawnDirectionalHitbox()
        {
            currentHitbox = Instantiate(hitboxPrefab, player.position, Quaternion.identity, transform);
            currentHitbox.transform.RotateAround(player.position, Vector3.up, ComputeHitboxDirection());
        }

        private void SpawnWhirlwindHitbox()
        {
            GameObject prefab = specialHitboxPrefab != null ? specialHitboxPrefab : hitboxPrefab;
            currentHitbox = Instantiate(prefab, player.position, Quaternion.identity, transform);

            if (specialHitboxPrefab != null || currentHitbox.transform.childCount == 0)
                return;

            Transform hitbox = currentHitbox.transform.GetChild(0);
            hitbox.localPosition = whirlwindHitboxLocalPosition;
            hitbox.localScale = whirlwindHitboxLocalScale;
        }

        private void SpawnJumpTagHitbox()
        {
            if (currentHitbox != null)
                RemoveHitbox();

            GameObject prefab = jumpTagHitboxPrefab != null ? jumpTagHitboxPrefab : hitboxPrefab;
            currentHitbox = Instantiate(prefab, player.position, Quaternion.identity, transform);

            if (jumpTagHitboxPrefab != null || currentHitbox.transform.childCount == 0)
                return;

            Transform hitbox = currentHitbox.transform.GetChild(0);
            hitbox.localPosition = jumpTagHitboxLocalPosition;
            hitbox.localScale = jumpTagHitboxLocalScale;
        }

        private float ComputeHitboxDirection()
        {
            float angle = player.LastLookDirection.ToSignedDegree();

            if (angle < 0)
                angle = 360.0f + angle;

            if (angle > 15.0f && angle <= 60.0f)
                return -45.0f;

            if (angle > 60.0f && angle <= 120.0f)
                return -100.0f;

            if (angle > 120.0f && angle < 165.0f)
                return -145.0f;

            if (angle >= 165.0f && angle <= 240.0f)
                return 180.0f;

            if (angle > 240.0f && angle <= 300.0f)
                return 90.0f;

            if ((angle > 300.0f && angle <= 360.0f) || (angle >= 0.0f && angle <= 15.0f))
                return 0.0f;

            return 90.0f;
        }

        public void RemoveHitbox()
        {
            if (currentHitbox != null)
            {
                Destroy(currentHitbox);
                currentHitbox = null;
            }
        }

        public void ForceSwordInHand()
        {
            if (currentlyHasSword)
                isSwordInHand = true;
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
