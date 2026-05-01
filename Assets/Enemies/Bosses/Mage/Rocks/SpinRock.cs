using PrimeTween;
using Tools_and_Scripts;
using UnityEngine;

public class SpinRock : MonoBehaviour
{
    public enum RockState
    {
        Hidden,
        Spinning,
        Bouncing,
        Locked
    }

    [SerializeField] private float moveSpeed;

    private SpriteRenderer spriteRenderer;
    private Sequence currentSequence;

    private float speedBoost;
    private float angle;
    private RockState currentState = RockState.Spinning;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        angle = Random.Range(0.0f, 360.0f);

        if (currentState != RockState.Locked)
        {
            HideRockInstant();
            DisplayRock();
        }
    }

    private void Update()
    {
        if (currentState == RockState.Spinning)
            Spin();
        else if (currentState == RockState.Bouncing)
            Bounce();
    }

    private void Spin()
    {
        Vector3 position = transform.position;
        float radius = position.ToVector2().magnitude;

        angle += (moveSpeed + speedBoost) * Time.deltaTime;
        Vector3 newPosition = new Vector3(Mathf.Cos(angle), 0.0f, Mathf.Sin(angle)) * radius;
        newPosition.y = position.y;

        transform.position = newPosition;
    }

    private void Bounce()
    {

    }

    public void SetState(RockState newState)
    {
        if (newState == currentState)
            return;

        RockState previousState = currentState;
        currentState = newState;

        if (currentState == RockState.Locked)
        {
            if (currentSequence.isAlive)
                currentSequence.Stop();
            return;
        }

        if (newState == RockState.Hidden)
            HideRock();
        else if (previousState == RockState.Hidden)
            DisplayRock();
    }

    private void DisplayRock(float delay = 0.0f)
    {
        if (currentSequence.isAlive)
            currentSequence.Stop();

        currentSequence = Sequence.Create()
            .Chain(Tween.LocalPositionY(transform, 0.0f, 2.0f, Ease.OutBack, startDelay: delay))
            .Group(Tween.Alpha(spriteRenderer, 1.0f, 0.5f));
    }

    private void HideRock()
    {
        if (currentSequence.isAlive)
            currentSequence.Stop();

        currentSequence = Sequence.Create()
                    .Chain(Tween.LocalPositionY(transform, -20.0f, 2.0f, Ease.InBack))
                    .Group(Tween.Alpha(spriteRenderer, 0.0f, 1.9f));
    }

    private void HideRockInstant()
    {
        Vector3 position = transform.localPosition;
        position.y = -20.0f;
        transform.localPosition = position;
        spriteRenderer.MakeTransparent();
    }

    public void SetMoveSpeedBoost(float boost)
    {
        speedBoost = boost;
    }
}
