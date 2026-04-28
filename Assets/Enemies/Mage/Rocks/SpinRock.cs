using UnityEngine;

public class SpinRock : MonoBehaviour
{
    [SerializeField] private float moveSpeed;

    private float angle;
    private bool isLocked;

    private void Start()
    {
        angle = Random.Range(0.0f, 360.0f);
    }

    private void Update()
    {
        if (isLocked)
            return;

        float radius = transform.position.magnitude;

        angle += moveSpeed * Time.deltaTime;
        transform.position = new Vector3(Mathf.Cos(angle), 0.0f, Mathf.Sin(angle)) * radius;
    }

    public void SetLockState(bool state)
    {
        isLocked = state;
    }
}
