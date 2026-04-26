using Camera_Movements;
using UnityEngine;

public class CamerasHolder : MonoBehaviour
{
    public Camera mainCamera;
    public Camera decorCamera;
    public CameraFollowPlayer cameraFollowPlayer;

    public static CamerasHolder instance;

    public void Awake()
    {
        instance = this;
    }
}
