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
        }

        private void StartShake()
        {
            StopAllCoroutines();
            StartCoroutine(Shake());
        }

        private IEnumerator Shake()
        {
            Vector3 newPosition = Vector3.zero;
            
            float timer = 0.0f;
            while (timer <= duration)
            {
                newPosition.x = Mathf.Sin(timer * frequency) * amplitudeX;
                newPosition.y = Mathf.Cos(timer * frequency) * amplitudeY;

                transform.localPosition = newPosition;

                yield return null;
                timer += Time.deltaTime;
            }
            
            transform.localPosition = startingPosition;
        }
    }
}
