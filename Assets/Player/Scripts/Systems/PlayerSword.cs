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
            player.playerParry.OnStartParry.AddListener(() => isSwordInHand = true);
            player.playerSit.OnStartSittingDown.AddListener(() => isSwordInHand = false);

            player.playerAttack.OnPlayerAttack.AddListener((_) => isSwordInHand = true);
            player.playerAttack.OnSpawnDamageBox.AddListener(SpawnHitbox);
            player.playerAttack.OnRemoveDamageBox.AddListener(RemoveHitbox);
        }

        private void SpawnHitbox()
        {
            if (currentHitbox != null)
                RemoveHitbox();

            currentHitbox = Instantiate(hitboxPrefab, player.position, Quaternion.identity, transform);
            currentHitbox.transform.RotateAround(player.position, Vector3.up, ComputeHitboxDirection());
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
