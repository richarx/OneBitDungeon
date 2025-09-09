using UnityEngine;

public class DestroyAfterDuration : MonoBehaviour
{
    [SerializeField] private float duration;
    
    private void Start()
    {
        Destroy(gameObject, duration);
    }

    public void SetDuration(float newDuration)
    {
        duration = newDuration;
    }

    private void OnDisable()
    {
        Destroy(gameObject);
    }
}
