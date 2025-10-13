using System.Collections;
using Player;
using Player.Scripts;
using Player.Sword_Hitboxes;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Tools_and_Scripts
{
    public class CameraShaker : MonoBehaviour
    {
        [SerializeField] private float amplitudeX;
        [SerializeField] private float amplitudeY;
        [SerializeField] private float frequency;
        [SerializeField] private float duration;
        
        private PlayerStateMachine player;

        private Vector3 startingPosition;
        
        private void Start()
        {
            startingPosition = transform.localPosition;
            player = PlayerStateMachine.instance;
            WeaponDamageTrigger.OnHitEnemy.AddListener((_) => StartShake());
            player.playerHealth.OnPlayerTakeDamage.AddListener((_) => StartShake(1.3f, 1.3f, 2.0f));
            player.playerParry.OnSuccessfulParry.AddListener(() => StartShake());
        }

        private void StartShake(float amplitudePower = 1.0f, float frequencyPower = 1.0f, float timePower = 1.0f)
        {
            StopAllCoroutines();
            StartCoroutine(Shake(amplitudePower, frequencyPower, timePower));
        }

        private IEnumerator Shake(float amplitudePower = 1.0f, float frequencyPower = 1.0f, float timePower = 1.0f)
        {
            Vector3 newPosition = Vector3.zero;
            
            float timer = 0.0f;
            while (timer <= duration * timePower)
            {
                newPosition.x = Mathf.Sin(timer * frequency * frequencyPower) * amplitudeX * amplitudePower;
                newPosition.y = Mathf.Cos(timer * frequency * frequencyPower) * amplitudeY * amplitudePower;

                transform.localPosition = newPosition;

                yield return null;
                timer += Time.deltaTime;
            }
            
            transform.localPosition = startingPosition;
        }
    }
}
