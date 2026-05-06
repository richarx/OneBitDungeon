using Player.Scripts;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class NpcEyes : MonoBehaviour
{
    [SerializeField] private SpriteRenderer eyes;
    [SerializeField] private Sprite front;
    [SerializeField] private Sprite frontClose;
    [SerializeField] private Sprite leftClose;
    [SerializeField] private Sprite left;
    [SerializeField] private Sprite rightClose;
    [SerializeField] private Sprite right;

    [Space]
    [SerializeField] private float lookDistance;
    [SerializeField] private float forwardThreshold;

    private PlayerStateMachine player;

    private void Start()
    {
        player = PlayerStateMachine.instance;
    }

    private void LateUpdate()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = (player.position - transform.position).magnitude;

        float forwardDot = Vector3.Dot(transform.forward * -1.0f, directionToPlayer);
        float rightDot = Vector3.Dot(transform.right, directionToPlayer);
        //Debug.Log($"Dot : {forwardDot}");

        if (forwardDot >= forwardThreshold)
            SetSprite(distanceToPlayer <= lookDistance ? frontClose : front);
        else if (forwardDot >= 0.0f)
            SetSprite(rightDot > 0.0f ? rightClose : leftClose);
        else
            SetSprite(rightDot > 0.0f ? right : left);
    }

    private void SetSprite(Sprite sprite)
    {
        eyes.sprite = sprite;
    }
}
