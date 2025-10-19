using Tools_and_Scripts;
using UnityEngine;

namespace Warning_Boxes
{
    public class CircularWarning : MonoBehaviour
    {
        [SerializeField] private float circleSpawnDuration;
        
        [Space]
        [SerializeField] private SpriteRenderer circle;
        [SerializeField] private SpriteRenderer background;
        
        [Space]
        [SerializeField] private Color startColor;
        [SerializeField] private Color endColor;
        [SerializeField] private Color cancelColor;
        
        private Vector2 position;
        private float radius;
        private float duration;
        
        private float spawnTimestamp;
        private float timeSinceSpawn => Time.time - spawnTimestamp;
        
        private float currentCircleDistance;
        private float circleVelocity;

        private float currentBackgroundDistance;
        private float backgroundVelocity;

        private bool isCanceled;

        private bool isSetup;
        
        public void Setup(Vector2 _position, float _radius, float _duration)
        {
            position = _position;
            radius = _radius;
            duration = _duration;

            spawnTimestamp = Time.time;
            transform.position = position.ToVector3();
            SetColor(startColor);
            
            //
            Destroy(gameObject, duration + 1.0f);
            //
            
            isSetup = true;
        }
        
        private void Update()
        {
            if (!isSetup || isCanceled)
                return;
            
            UpdateLines();

            if (timeSinceSpawn >= circleSpawnDuration)
                UpdateBackground();
        }
        
        private void UpdateLines()
        {
            currentCircleDistance = Mathf.SmoothDamp(currentCircleDistance, radius + 0.5f, ref circleVelocity, circleSpawnDuration);
            currentCircleDistance = Mathf.Min(currentCircleDistance, radius);
            
            circle.transform.localScale = Vector3.one * (currentCircleDistance * 2.0f);
        }
        
        private void UpdateBackground()
        {
            currentBackgroundDistance = Mathf.SmoothDamp(currentBackgroundDistance, radius + 0.5f, ref backgroundVelocity, duration - circleSpawnDuration);
            currentBackgroundDistance = Mathf.Min(currentBackgroundDistance, radius);
            background.transform.localScale = Vector3.one * (currentBackgroundDistance * 2.0f);
        }
        
        private void SetColor(Color target)
        {
            circle.color = target;
            background.color = target;
        }
        
        public bool IsFull()
        {
            return currentBackgroundDistance >= radius;
        }

        public void Trigger()
        {
            SetColor(endColor);
            FadeOut(0.1f);
            Destroy(gameObject, 0.25f);
        }

        public void DestroyBox()
        {
            FadeOut();
            Destroy(gameObject, 0.15f);
        }

        public void Cancel()
        {
            SetColor(cancelColor);
            FadeOut();
            Destroy(gameObject, 0.15f);
        }

        private void FadeOut(float delay = 0.0f)
        {
            StartCoroutine(Tools.Fade(background, 0.15f, false, background.color.a, delay));
            StartCoroutine(Tools.Fade(circle, 0.15f, false, circle.color.a, delay));
        }
    }
}
