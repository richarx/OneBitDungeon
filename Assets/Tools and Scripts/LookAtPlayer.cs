using Player;
using Player.Scripts;
using UnityEngine;

namespace Tools_and_Scripts
{
    public class LookAtPlayer : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer sr;
        [SerializeField] private bool isAllowedToLean;
        
        private Transform player;

        private void Start()
        {
            player = PlayerStateMachine.instance.transform;
            
            if (sr == null)
                sr = GetComponent<SpriteRenderer>();
            sr.flipX = true;
        }

        private void LateUpdate()
        {
            Vector3 position = player.position;
            
            if (!isAllowedToLean)
                position.y = transform.position.y;
            
            transform.LookAt(position, Vector3.up);
        }
    }
}
