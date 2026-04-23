using Enemies.Scripts;
using PrimeTween;
using UnityEngine;

public class SqueezeOnHit : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 strength;
    [SerializeField] private float duration;
    [SerializeField] private float frequency;

    private void Start()
    {
        GetComponent<Damageable>().OnTakeDamage.AddListener(Squeeze);
    }

    private void Squeeze()
    {
        Sequence.Create()
            .Chain(Tween.PunchScale(target, strength, duration, frequency));
    }
}
