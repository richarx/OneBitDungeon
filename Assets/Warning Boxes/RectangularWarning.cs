using Tools_and_Scripts;
using UnityEngine;

namespace Warning_Boxes
{
    public class RectangularWarning : MonoBehaviour, IWarningBox
    {
        [SerializeField] private float lineSpawnDuration;
        [SerializeField] private LineRenderer lineL;
        [SerializeField] private LineRenderer lineR;
        [SerializeField] private SpriteRenderer background;
        [SerializeField] private Color startColor;
        [SerializeField] private Color endColor;
        [SerializeField] private Color cancelColor;

        private Vector2 position;
        private Vector2 direction;
        private float width;
        private float length;
        private float duration;

        private float spawnTimestamp;
        private float timeSinceSpawn => Time.time - spawnTimestamp;
        
        private float currentLineDistance;
        private float lineVelocity;

        private float currentBackgroundDistance;
        private float backgroundVelocity;

        private bool isCanceled;

        private bool isSetup;
        
        public void Setup(Vector2 _position, Vector2 _direction, float _width, float _length, float _duration)
        {
            position = _position;
            direction = _direction;
            width = _width;
            length = _length;
            duration = _duration;

            spawnTimestamp = Time.time;
            transform.position = position.ToVector3();
            background.transform.localRotation = direction.AddAngleToDirection(-90.0f).ToRotation();
            SetColor(startColor);
            
            background.size = new Vector2(width, 0.0f);
            
            isSetup = true;
        }

        private void Update()
        {
            if (!isSetup || isCanceled)
                return;
            
            UpdateLines();

            if (timeSinceSpawn >= lineSpawnDuration)
                UpdateBackground();
        }

        private void SetColor(Color target)
        {
            lineL.startColor = target;
            lineL.endColor = target;
            lineR.startColor = target;
            lineR.endColor = target;
            background.color = target;
        }

        private void UpdateLines()
        {
            currentLineDistance = Mathf.SmoothDamp(currentLineDistance, length + 0.1f, ref lineVelocity, lineSpawnDuration);
            currentLineDistance = Mathf.Min(currentLineDistance, length);
            
            Vector2 start_L = position + direction.AddAngleToDirection(90.0f) * (width / 2.0f);
            Vector2 end_L = start_L + direction * currentLineDistance;

            lineL.positionCount = 2;
            lineL.SetPositions(new [] { start_L.ToVector3(), end_L.ToVector3()});
            
            Vector2 start_R = position + direction.AddAngleToDirection(-90.0f) * (width / 2.0f);
            Vector2 end_R = start_R + direction * currentLineDistance;
            
            lineR.positionCount = 2;
            lineR.SetPositions(new [] { start_R.ToVector3(), end_R.ToVector3()});
        }
        
        private void UpdateBackground()
        {
            currentBackgroundDistance = Mathf.SmoothDamp(currentBackgroundDistance, length + 0.1f, ref backgroundVelocity, duration - lineSpawnDuration);
            background.size = new Vector2(width, currentBackgroundDistance);
        }

        public void UpdateDirection(Vector2 _direction)
        {
            direction = _direction;
            background.transform.localRotation = direction.AddAngleToDirection(-90.0f).ToRotation();
        }

        public bool IsFull()
        {
            return currentBackgroundDistance >= length;
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
            StartCoroutine(Tools.Fade(lineL, 0.15f, false, lineL.startColor.a, delay));
            StartCoroutine(Tools.Fade(lineR, 0.15f, false, lineR.startColor.a, delay));
        }
    }
}
