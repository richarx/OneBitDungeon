using System.Collections;
using Player.Sword_Hitboxes;
using Sirenix.OdinInspector;
using Tools_and_Scripts;
using UnityEngine;
using UnityEngine.Serialization;

namespace Player.Scripts
{
    public class PlayerVfx : MonoBehaviour
    {
        [FoldoutGroup(CombatVfxGroup, true), BoxGroup(HitGroup), LabelText("Spark Prefab"), SerializeField]
        private GameObject hitSparkPrefab;

        [FoldoutGroup(CombatVfxGroup, true), BoxGroup(HitGroup), LabelText("Spark Height"), MinValue(0.0f), SerializeField]
        private float sparkHeight;

        [FoldoutGroup(CombatVfxGroup, true), BoxGroup(HitGroup), LabelText("Spark Distance"), MinValue(0.0f), SerializeField]
        private float sparkDistance;

        [FoldoutGroup(CombatVfxGroup, true), BoxGroup(HitGroup), LabelText("Slash Prefab"), SerializeField]
        private GameObject hitSlashPrefab;

        [FoldoutGroup(CombatVfxGroup, true), BoxGroup(HitGroup), LabelText("Slash Height"), MinValue(0.0f), SerializeField]
        private float slashHeight;

        [FoldoutGroup(CombatVfxGroup, true), BoxGroup(HitGroup), LabelText("Slash Distance"), MinValue(0.0f), SerializeField]
        private float slashDistance;

        [FoldoutGroup(CombatVfxGroup, true), BoxGroup(HitGroup), LabelText("Enemy Hit Duration"), MinValue(0.0f), SerializeField]
        private float hitFreezeDuration = 0.05f;

        [FoldoutGroup(CombatVfxGroup, true), BoxGroup(SwordSlashGroup), LabelText("First Prefab"), SerializeField]
        private GameObject swordSlashPrefab;

        [FoldoutGroup(CombatVfxGroup, true), BoxGroup(SwordSlashGroup), LabelText("Second Prefab"), SerializeField]
        private GameObject swordSecondSlashPrefab;

        [FoldoutGroup(CombatVfxGroup, true), BoxGroup(SwordSlashGroup), LabelText("Spawn Delay"), MinValue(0.0f), SerializeField]
        private float swordSlashDelay;

        [FoldoutGroup(CombatVfxGroup, true), BoxGroup(SwordSlashGroup), LabelText("Height"), MinValue(0.0f), SerializeField]
        private float swordSlashHeight;

        [FoldoutGroup(CombatVfxGroup, true), BoxGroup(SwordSlashGroup), LabelText("Distance"), MinValue(0.0f), SerializeField]
        private float swordSlashDistance;

        [FoldoutGroup(CombatVfxGroup, true), BoxGroup(ParryGroup), LabelText("Prefab"), SerializeField]
        private GameObject parryVfx;

        [FoldoutGroup(CombatVfxGroup, true), BoxGroup(ParryGroup), LabelText("Parry Duration"), MinValue(0.0f), SerializeField]
        private float parryFreezeDuration = 0.03f;

        [FoldoutGroup(MovementVfxGroup, true), BoxGroup(RollGroup), LabelText("Prefab"), SerializeField]
        private GameObject rollVfx;

        [FoldoutGroup(MovementVfxGroup, true), BoxGroup(JumpGroup), LabelText("Start Prefab"), SerializeField]
        private GameObject jumpStartVfx;

        [FoldoutGroup(MovementVfxGroup, true), BoxGroup(JumpGroup), LabelText("Land Prefab"), SerializeField]
        private GameObject jumpLandVfx;

        [FoldoutGroup(CombatVfxGroup, true), BoxGroup(HurtGroup), LabelText("Prefab"), SerializeField]
        private GameObject hurtVfx;

        [FoldoutGroup(CombatVfxGroup, true), BoxGroup(HurtGroup), LabelText("Duration"), MinValue(0.0f), SerializeField]
        private float hurtFreezeDuration = 0.05f;

        [FoldoutGroup(FreezeGroup, true), LabelText("Delay"), MinValue(0.0f), SerializeField]
        private float freezeDelay;


        private PlayerStateMachine player;

        private bool isTimeFrozen;
        private bool hasSubscribed;
        private float timeScaleBeforeFreeze = 1.0f;

        private void Start()
        {
            player = PlayerStateMachine.instance;
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            RestoreTimeScale();
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            if (player == null)
                return;

            WeaponDamageTrigger.OnHitEnemy.AddListener(HandleHitEnemy);
            player.playerHealth.OnPlayerTakeDamage.AddListener(HandlePlayerTakeDamage);
            player.playerAttack.OnPlayerAttack.AddListener(HandlePlayerAttack);
            player.playerParry.OnSuccessfulParry.AddListener(HandleSuccessfulParry);
            player.playerRoll.OnStartRoll.AddListener(SpawnRollVfx);
            player.playerJump.OnStartJump.AddListener(SpawnStartJumpVfx);
            player.playerJump.OnLandJump.AddListener(SpawnLandJumpVfx);
            hasSubscribed = true;
        }

        private void UnsubscribeFromEvents()
        {
            if (!hasSubscribed)
                return;

            WeaponDamageTrigger.OnHitEnemy.RemoveListener(HandleHitEnemy);

            if (player != null)
            {
                player.playerHealth.OnPlayerTakeDamage.RemoveListener(HandlePlayerTakeDamage);
                player.playerAttack.OnPlayerAttack.RemoveListener(HandlePlayerAttack);
                player.playerParry.OnSuccessfulParry.RemoveListener(HandleSuccessfulParry);
                player.playerRoll.OnStartRoll.RemoveListener(SpawnRollVfx);
                player.playerJump.OnStartJump.RemoveListener(SpawnStartJumpVfx);
                player.playerJump.OnLandJump.RemoveListener(SpawnLandJumpVfx);
            }

            hasSubscribed = false;
        }

        private void HandleHitEnemy(Vector3 position)
        {
            SpawnSwordHit(position);
            FreezeTime(hitFreezeDuration);
        }

        private void HandlePlayerTakeDamage(Vector3 direction)
        {
            FreezeTime(hurtFreezeDuration);
            SpawnHurtVfx(direction);
        }

        private void HandlePlayerAttack(AttackPayload attackPayload)
        {
            StartCoroutine(WaitAndSpawnSwordSlash(attackPayload));
        }

        private void HandleSuccessfulParry()
        {
            SpawnParryVfx();
            FreezeTime(parryFreezeDuration);
        }

        private void SpawnHurtVfx(Vector3 direction)
        {
            Vector3 position = player.position + Vector3.up + (direction * 3.0f);
            GameObject hurt = InstantiateVfx(hurtVfx, position, Quaternion.identity);
            if (direction.x < 0.0f)
                hurt.transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
        }

        private void SpawnRollVfx()
        {
            GameObject roll = InstantiateVfx(rollVfx, player.position, Quaternion.identity);
            if (!player.playerRoll.IsRollingLeft)
                roll.transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
        }

        private void SpawnStartJumpVfx()
        {
            GameObject jump = InstantiateVfx(jumpStartVfx, player.position, Quaternion.identity);
            if (player.LastLookDirection.x > 0.0f)
                jump.transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
        }

        private void SpawnLandJumpVfx()
        {
            InstantiateVfx(jumpLandVfx, player.position, Quaternion.identity);
        }

        private void SpawnParryVfx()
        {
            InstantiateVfx(parryVfx, player.position, Quaternion.identity);
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

            Transform slash = InstantiateVfx(prefab, position, Quaternion.identity).transform;

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
            Transform slash = InstantiateVfx(swordSlashPrefab, position, Quaternion.identity).transform;

            slash.RotateAround(slash.position, Vector3.up, 360.0f - angle);
        }

        private Vector3 DirectionFromAngle(float angle)
        {
            float radians = angle * Mathf.Deg2Rad;
            return new Vector3(Mathf.Cos(radians), 0.0f, Mathf.Sin(radians));
        }

        private void FreezeTime(float duration)
        {
            if (duration > 0.0f && !isTimeFrozen && !player.playerHealth.IsDead)
                StartCoroutine(FreezeTimeCoroutine(duration));
        }

        private IEnumerator FreezeTimeCoroutine(float duration)
        {
            isTimeFrozen = true;
            yield return new WaitForSecondsRealtime(freezeDelay);
            timeScaleBeforeFreeze = Time.timeScale;
            Time.timeScale = 0.0f;
            yield return new WaitForSecondsRealtime(duration);
            RestoreTimeScale();
        }

        private void SpawnSwordHit(Vector3 position)
        {
            Vector3 directionToEnemy = (position - player.position).normalized;

            Vector3 sparkPosition = position + (Vector3.up * sparkHeight) + (directionToEnemy * sparkDistance);
            InstantiateVfx(hitSparkPrefab, sparkPosition, directionToEnemy.ToVector2().AddRandomAngleToDirection(-15.0f, 15.0f).ToRotation());

            Vector3 slashPosition = player.position + (Vector3.up * slashHeight) + (directionToEnemy * slashDistance);
            InstantiateVfx(hitSlashPrefab, slashPosition, directionToEnemy.ToVector2().AddRandomAngleToDirection(-15.0f, 15.0f).ToRotation());
        }

        private GameObject InstantiateVfx(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            GameObject instance = Instantiate(prefab, position, rotation);
            instance.UseUnscaledTime();
            return instance;
        }

        private void RestoreTimeScale()
        {
            if (!isTimeFrozen)
                return;

            Time.timeScale = timeScaleBeforeFreeze;
            isTimeFrozen = false;
        }

        #region Inspector Names


        private const string CombatVfxGroup = "Combat VFX";
        private const string HitGroup = CombatVfxGroup + "/Enemy Hit";
        private const string SwordSlashGroup = CombatVfxGroup + "/Sword Slash";
        private const string ParryGroup = CombatVfxGroup + "/Parry";
        private const string HurtGroup = CombatVfxGroup + "/Hurt";
        private const string MovementVfxGroup = "Movement VFX";
        private const string RollGroup = MovementVfxGroup + "/Roll";
        private const string JumpGroup = MovementVfxGroup + "/Jump";
        private const string FreezeGroup = "Freeze Timings";
        #endregion
    }
}
