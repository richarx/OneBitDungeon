using UnityEngine;

namespace Warning_Boxes
{
    public class WarningBoxes : MonoBehaviour
    {
        [SerializeField] private RectangularWarning rectangularPrefab;
        
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
    }
}
