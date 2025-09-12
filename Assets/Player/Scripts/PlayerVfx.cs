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
        [SerializeField] private float freezeDelay;
        [SerializeField] private float freezeDuration;
        
        private PlayerStateMachine player;

        private bool isTimeFrozen;
        
        private void Start()
        {
            player = PlayerStateMachine.instance;
            WeaponDamageTrigger.OnHitEnemy.AddListener(SpawnSwordHit);
            WeaponDamageTrigger.OnHitEnemy.AddListener((position) =>
            {
                if (!isTimeFrozen)
                    StartCoroutine(FreezeTime());
            });
        }

        private IEnumerator FreezeTime()
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
