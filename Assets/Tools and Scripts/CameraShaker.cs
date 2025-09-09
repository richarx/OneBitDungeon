using System.Collections;
using Player;
using Player.Scripts;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Tools_and_Scripts
{
    public class CameraShaker : MonoBehaviour
    {
        [SerializeField] private float amplitudeX;
        [SerializeField] private float amplitudeY;
        [SerializeField] private float frequency;
        
        private PlayerStateMachine player;

        private Vector3 startingPosition;
        
        private void Start()
        {
            startingPosition = transform.localPosition;
            player = PlayerStateMachine.instance;
        }

        private void StartSlide()
        {
            StopAllCoroutines();
            //StartCoroutine(ShakeDuringSlide());
        }

        /*
        private IEnumerator ShakeDuringSlide()
        {
            while (player.currentBehaviour.GetBehaviourType() == BehaviourType.Slide)
            {
                Vector3 newPosition = Vector3.zero;

                newPosition.x = Random.Range(-amplitudeX, amplitudeX);
                newPosition.y = Random.Range(-amplitudeY, amplitudeY);

                transform.localPosition = newPosition;

                yield return new WaitForSeconds(0.05f);
            }
            transform.localPosition = startingPosition;
        }
        */

        private void StopSlide()
        {
            StopAllCoroutines();
            transform.localPosition = startingPosition;
        }
    }
}
