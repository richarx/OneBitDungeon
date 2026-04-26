using PrimeTween;
using UnityEngine;

public class CameraBossRoom : MonoBehaviour
{
    [SerializeField] private Transform starting;
    [SerializeField] private Transform target;
    [SerializeField] private float duration;
    [SerializeField] private Ease ease;
    [SerializeField] private float delay;

    private void Start()
    {
        transform.position = starting.position;
        transform.rotation = starting.rotation;

        Sequence.Create()
            .Group(Tween.Position(transform, target.position, duration, ease, startDelay: delay))
            .Group(Tween.Rotation(transform, target.rotation, duration, ease, startDelay: delay));
    }
}
