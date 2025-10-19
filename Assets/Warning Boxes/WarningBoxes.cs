using UnityEngine;

namespace Warning_Boxes
{
    public class WarningBoxes : MonoBehaviour
    {
        [SerializeField] private RectangularWarning rectangularPrefab;
        [SerializeField] private CircularWarning circularPrefab;
        
        public static WarningBoxes instance;

        private void Awake()
        {
            instance = this;
        }

        public RectangularWarning SpawnRectangularWarning(Vector2 position, Vector2 direction, float width, float length, float duration)
        {
            RectangularWarning warning = Instantiate(rectangularPrefab);
            warning.Setup(position, direction, width, length, duration);

            return warning;
        }
        
        public CircularWarning SpawnCircularWarning(Vector2 position, float radius, float duration)
        {
            CircularWarning warning = Instantiate(circularPrefab);
            warning.Setup(position, radius, duration);

            return warning;
        }
    }
}
