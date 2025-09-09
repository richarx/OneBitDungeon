using Player;
using Player.Scripts;
using UnityEngine;

namespace Tools_and_Scripts
{
    public class LookAtPlayerWithRotation : MonoBehaviour
    {
        private Transform player;
        private SpriteRenderer sr;

        private void Start()
        {
            player = PlayerStateMachine.instance.transform;
            sr = GetComponent<SpriteRenderer>();
            sr.flipX = true;

            float randomAngle = Random.Range(-100.0f, 100.0f);
            
            transform.LookAt(player.position, Vector3.up);
            transform.Rotate(Vector3.forward, randomAngle, Space.Self);
        }
    }
}
