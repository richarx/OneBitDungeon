using System;
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
        [SerializeField] private GameObject jumpStartVfx;
        [SerializeField] private GameObject jumpLandVfx;

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
                FreezeTime(freezeDuration);
            });
            player.playerHealth.OnPlayerTakeDamage.AddListener((direction) =>
            {
                FreezeTime(freezeDuration);
                SpawnHurtVfx(direction);
            });
            player.playerAttack.OnPlayerAttack.AddListener((attackPayload) =>
            {
                StartCoroutine(WaitAndSpawnSwordSlash(attackPayload));
            });
            player.playerParry.OnSuccessfulParry.AddListener(() =>
            {
                SpawnParryVfx();
                FreezeTime(0.03f);
            });
            player.playerRoll.OnStartRoll.AddListener(SpawnRollVfx);
            player.playerJump.OnStartJump.AddListener(SpawnStartJumpVfx);
            player.playerJump.OnLandJump.AddListener(SpawnLandJumpVfx);
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

        private void SpawnStartJumpVfx()
        {
            GameObject jump = Instantiate(jumpStartVfx, player.position, Quaternion.identity);
            if (player.LastLookDirection.x > 0.0f)
                jump.transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
        }

        private void SpawnLandJumpVfx()
        {
            GameObject jump = Instantiate(jumpLandVfx, player.position, Quaternion.identity);
        }

        private void SpawnParryVfx()
        {
            Instantiate(parryVfx, player.position, Quaternion.identity);
        }

        private IEnumerator WaitAndSpawnSwordSlash(AttackPayload attackPayload)
        {
            yield return new WaitForSeconds(swordSlashDelay);

            if (attackPayload.Type == AttackType.Special)
            {
                SpawnSwordWhirlwindSlash();
                yield break;
            }

            SpawnDirectionalSwordSlash(player.playerAttack.IsSecondAttack);
        }

        private void SpawnDirectionalSwordSlash(bool isSecondAttack)
        {
            GameObject prefab = isSecondAttack ? swordSecondSlashPrefab : swordSlashPrefab;
            Vector3 position = player.position + (Vector3.up * swordSlashHeight) + (player.LastLookDirection.ToVector3() * swordSlashDistance);

            Transform slash = Instantiate(prefab, position, Quaternion.identity).transform;

            slash.RotateAround(slash.position, Vector3.up, 360.0f - player.LastLookDirection.ToDegree());
        }

        private void SpawnSwordWhirlwindSlash()
        {
            if (swordSlashPrefab == null)
                return;

            SpawnSwordSlashAtAngle(0.0f);
            SpawnSwordSlashAtAngle(90.0f);
            SpawnSwordSlashAtAngle(180.0f);
            SpawnSwordSlashAtAngle(270.0f);
        }

        private void SpawnSwordSlashAtAngle(float angle)
        {
            Vector3 direction = DirectionFromAngle(angle);
            Vector3 position = player.position + (Vector3.up * swordSlashHeight) + (direction * swordSlashDistance);
            Transform slash = Instantiate(swordSlashPrefab, position, Quaternion.identity).transform;

            slash.RotateAround(slash.position, Vector3.up, 360.0f - angle);
        }

        private Vector3 DirectionFromAngle(float angle)
        {
            float radians = angle * Mathf.Deg2Rad;
            return new Vector3(Mathf.Cos(radians), 0.0f, Mathf.Sin(radians));
        }

        private void FreezeTime(float duration)
        {
            if (!isTimeFrozen && !player.playerHealth.IsDead)
                StartCoroutine(FreezeTimeCoroutine(duration));
        }

        private IEnumerator FreezeTimeCoroutine(float duration)
        {
            isTimeFrozen = true;
            yield return new WaitForSecondsRealtime(freezeDelay);
            Time.timeScale = 0.0f;
            yield return new WaitForSecondsRealtime(duration);
            Time.timeScale = 1.0f;
            isTimeFrozen = false;
        }

        private void SpawnSwordHit(Vector3 position)
        {
            Vector3 directionToEnemy = (position - player.position).normalized;

            Vector3 sparkPosition = position + (Vector3.up * sparkHeight) + (directionToEnemy * sparkDistance);
            Instantiate(hitSparkPrefab, sparkPosition, directionToEnemy.ToVector2().AddRandomAngleToDirection(-15.0f, 15.0f).ToRotation());

            Vector3 slashPosition = player.position + (Vector3.up * slashHeight) + (directionToEnemy * slashDistance);
            Instantiate(hitSlashPrefab, slashPosition, directionToEnemy.ToVector2().AddRandomAngleToDirection(-15.0f, 15.0f).ToRotation());
        }
    }
}
