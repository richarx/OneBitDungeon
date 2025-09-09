using UnityEngine;

namespace Tools_and_Scripts.Tween
{
    public class GlideTween : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float glideSpeed;
        [SerializeField] private float glideAmplitude;

        private float startingHeight;
        private float targetHeight;
        private bool isGoingDown;

        private float velocity;
        
        private void Start()
        {
            startingHeight = target.localPosition.y;
        }

        private void Update()
        {
            if (HasReachedTarget())
                ComputeNewTarget();

            MoveTowardsTarget();
        }

        private bool HasReachedTarget()
        {
            float current = target.localPosition.y;

            return isGoingDown ? current <= (targetHeight + 0.1f) : current >= (targetHeight - 0.1f);
        }

        private void ComputeNewTarget()
        {
            isGoingDown = !isGoingDown;
            targetHeight = startingHeight + glideAmplitude * (isGoingDown ?  -1.0f : 1.0f);
        }

        private void MoveTowardsTarget()
        {
            Vector3 position = target.localPosition;
            
            float newHeight = Mathf.SmoothDamp(position.y, targetHeight, ref velocity, glideSpeed);
            target.localPosition = new Vector3(position.x, newHeight, position.z);
        }
    }
}
