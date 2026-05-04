using PrimeTween;
using UnityEngine;

namespace Player.Scripts
{
    public class SwapPlayerColor : MonoBehaviour
    {
        private enum SpriteState
        {
            Initial,
            Staggered,
            Exhausted
        }

        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private float transitionDuration;
        [SerializeField] private Ease transitionEase;

        [Space]
        [SerializeField] private Color staggerTargetColor;
        [SerializeField] private float staggerDuration;

        [Space]
        [SerializeField] private Color staminaTargetColor;

        private PlayerHealth playerHealth;
        private PlayerStamina playerStamina;
        private Color startingColor;
        private SpriteState currentState;
        private Sequence currentSequence;

        private void Start()
        {
            startingColor = spriteRenderer.color;

            playerHealth = GetComponent<PlayerHealth>();
            playerStamina = GetComponent<PlayerStamina>();
        }

        private void LateUpdate()
        {
            SpriteState newState = ComputeCurrentState();

            if (currentState != newState)
                TransitionToNewState(newState);
        }

        private void TransitionToNewState(SpriteState newState)
        {
            currentState = newState;

            Color targetColor = ComputeColorFromState(newState);

            if (currentSequence.isAlive)
                currentSequence.Stop();

            currentSequence = Sequence.Create()
                .Group(Tween.Color(spriteRenderer, targetColor, transitionDuration, transitionEase));
        }

        private Color ComputeColorFromState(SpriteState newState)
        {
            switch (newState)
            {
                case SpriteState.Staggered:
                    return staggerTargetColor;
                case SpriteState.Exhausted:
                    return staminaTargetColor;
                default:
                case SpriteState.Initial:
                    return startingColor;
            }
        }

        private SpriteState ComputeCurrentState()
        {
            if (Time.time - playerHealth.lastHitTimestamp <= staggerDuration)
                return SpriteState.Staggered;

            if (playerStamina.IsEmpty)
                return SpriteState.Exhausted;

            return SpriteState.Initial;
        }
    }
}
