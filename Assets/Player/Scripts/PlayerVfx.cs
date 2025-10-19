using System.Collections;
using Player.Sword_Hitboxes;
using Tools_and_Scripts;
using UnityEngine;

namespace Player.Scripts
{
    public class PlayerVfx : MonoBehaviour
    {
        [SerializeField] private GameObject hitSparkPrefab;
        [SerializeField] private float sparkHeight;
        [SerializeField] private float sparkDistance;
        
        [Space]
        [SerializeField] private GameObject hitSlashPrefab;
        [SerializeField] private float slashHeight;
        [SerializeField] private float slashDistance;
        
        [Space]
        [SerializeField] private GameObject swordSlashPrefab;
        [SerializeField] private GameObject swordSecondSlashPrefab;
        [SerializeField] private float swordSlashDelay;
        [SerializeField] private float swordSlashHeight;
        [SerializeField] private float swordSlashDistance;

        [Space] 
        [SerializeField] private GameObject parryVfx;
        
        [Space] 
        [SerializeField] private GameObject rollVfx;
        
        [Space] 
        [SerializeField] private GameObject hurtVfx;

        [Space] 
        [SerializeField] private float freezeDelay;
        [SerializeField] private float freezeDuration;
        
        private PlayerStateMachine player;

        private bool isTimeFrozen;
        
        private void Start()
        {
            player = PlayerStateMachine.instance;
            WeaponDamageTrigger.OnHitEnemy.AddListener((position) =>
            {
                SpawnSwordHit(position);
                FreezeTime();
            });
            player.playerHealth.OnPlayerTakeDamage.AddListener((direction) =>
            {
                FreezeTime();
                SpawnHurtVfx(direction);
            });
            player.playerAttack.OnPlayerAttack.AddListener((_) =>
            {
                StartCoroutine(WaitAndSpawnSwordSlash());
            });
            player.playerParry.OnSuccessfulParry.AddListener(() =>
            {
                SpawnParryVfx();
                FreezeTime();
            });
            player.playerRoll.OnStartRoll.AddListener(SpawnRollVfx);
        }

        private void SpawnHurtVfx(Vector3 direction)
        {
            Vector3 position = player.position + Vector3.up + (direction * 3.0f);
            GameObject hurt = Instantiate(hurtVfx, position, Quaternion.identity);
            if (direction.x < 0.0f)
                hurt.transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
        }

        private void SpawnRollVfx()
        {
            GameObject roll = Instantiate(rollVfx, player.position, Quaternion.identity);
            if (!player.playerRoll.IsRollingLeft)
                roll.transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
        }

        private void SpawnParryVfx()
        {
            Instantiate(parryVfx, player.position, Quaternion.identity);
        }

        private IEnumerator WaitAndSpawnSwordSlash()
        {
            yield return new WaitForSeconds(swordSlashDelay);

            bool isSecondAttack = player.playerAttack.IsSecondAttack;
            GameObject prefab = isSecondAttack ? swordSecondSlashPrefab : swordSlashPrefab;
            Vector3 position = player.position + (Vector3.up * swordSlashHeight) + (player.LastLookDirection.ToVector3() * swordSlashDistance);
            
            Transform slash = Instantiate(prefab, position, Quaternion.identity).transform;

            slash.RotateAround(slash.position, Vector3.up, 360.0f - player.LastLookDirection.ToDegree());
        }

        private void FreezeTime()
        {
            if (!isTimeFrozen && !player.playerHealth.IsDead)
                StartCoroutine(FreezeTimeCoroutine());
        }
        
        private IEnumerator FreezeTimeCoroutine()
        {
            isTimeFrozen = true;
            yield return new WaitForSecondsRealtime(freezeDelay);
            Time.timeScale = 0.0f;
            yield return new WaitForSecondsRealtime(freezeDuration);
            Time.timeScale = 1.0f;
            isTimeFrozen = false;
        }

        private void SpawnSwordHit(Vector3 position)
        {
            Vector3 directionToEnemy = (position - player.position).normalized;
            
            Vector3 sparkPosition = position + (Vector3.up * sparkHeight) + (directionToEnemy * sparkDistance);
            Instantiate(hitSparkPrefab, sparkPosition, directionToEnemy.ToVector2().AddRandomAngleToDirection(-15.0f, 15.0f).ToRotation());

            Vector3 slashPosition = player.position + (Vector3.up * slashHeight) + (directionToEnemy * slashDistance);
            Instantiate(hitSlashPrefab, slashPosition, directionToEnemy.ToVector2().ToRotation());
        }
    }
}
