using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    [SerializeField] private Transform target;

    private Transform cameraTransform;

    private void Start()
    {
        cameraTransform = CamerasHolder.instance.transform;
    }

    private void LateUpdate()
    {
        target.rotation = cameraTransform.rotation;
    }
}
