using UnityEngine;
using UnityEngine.InputSystem;

namespace Tools_and_Scripts
{
    public class CameraScreenPosition : MonoBehaviour
    {
        public static CameraScreenPosition instance;

        private Camera mainCamera;
        
        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            mainCamera = Camera.main;
        }

        public Vector3 WorldToScreen(Vector3 position)
        {
            return mainCamera.WorldToScreenPoint(position);
        }
        
        public Vector3 ScreenToWorld(Vector2 position)
        {
            Vector3 screenPosition = new Vector3(position.x, position.y, mainCamera.nearClipPlane);
            
            return mainCamera.ScreenToWorldPoint(screenPosition);
        }

        public Vector2 GetScreenCenterPosition()
        {
            return new Vector2(mainCamera.pixelWidth / 2.0f, mainCamera.pixelHeight / 2.0f);
        }

        public Vector2 ScreenPointToLocalPointInRectangle(RectTransform rect, Vector2 position)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, position, mainCamera, out Vector2 result);
            return result;
        }
        
        public Vector2 GetMousePosition(RectTransform rect)
        {
            return ScreenPointToLocalPointInRectangle(rect, Mouse.current.position.value);
        }
    }
}
