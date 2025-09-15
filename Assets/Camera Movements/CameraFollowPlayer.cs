using Player.Scripts;
using UnityEngine;

namespace Camera_Movements
{
    public class CameraFollowPlayer : MonoBehaviour
    {
        [Header("Vertical")]
        [SerializeField] private float amplitude;
        [SerializeField] private float frequency;
        
        [Space]
        [Header("Horizontal")]
        [SerializeField] private float maxDistanceFromCenter;
        [SerializeField] private float smoothTime;

        private PlayerStateMachine player;

        private float velocity;
        
        private void Start()
        {
            player = PlayerStateMachine.instance;
        }

        private void LateUpdate()
        {
            float x = UpdateHorizontalPosition();
            float y = UpdateVerticalPosition();

            transform.localPosition = new Vector3(x, y, 0.0f);
        }

        private float UpdateVerticalPosition()
        {
            return amplitude * Mathf.Sin(Time.time * frequency);
        }
        
        private float UpdateHorizontalPosition()
        {
            float target = Mathf.Clamp(player.position.x, -maxDistanceFromCenter, maxDistanceFromCenter);

            return Mathf.SmoothDamp(transform.position.x, target, ref velocity, smoothTime);
        }
    }
}
