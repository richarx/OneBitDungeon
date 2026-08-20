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

        [Space]
        [SerializeField] private Color staminaTargetColor;

        [Space]
        [SerializeField][ColorUsage(true, true)] private Color materialStaggerTargetColor;

        private PlayerData playerData;
        private PlayerHealth playerHealth;
        private PlayerStamina playerStamina;

        private Material material;
        private int materialColorId;

        private Color startingColor;
        private Color startingMaterialColor;


        private SpriteState currentState;
        private Sequence currentSequence;

        private void Start()
        {
            startingColor = spriteRenderer.color;
            material = spriteRenderer.material;
            materialColorId = Shader.PropertyToID("_Emission");
            startingMaterialColor = material.GetColor(materialColorId);

            playerData = PlayerStateMachine.instance.playerData;
            playerHealth = GetComponent<PlayerHealth>();
            playerStamina = GetComponent<PlayerStamina>();

            currentState = SpriteState.Initial;
        }

        private void LateUpdate()
        {
            SpriteState newState = ComputeCurrentState();

            if (currentState != newState)
                TransitionToNewState(newState);
        }

        private void TransitionToNewState(SpriteState newState)
        {
            SpriteState previousState = currentState;
            currentState = newState;

            Color targetColor = ComputeColorFromState(newState);

            if (currentSequence.isAlive)
                currentSequence.Stop();

            currentSequence = Sequence.Create(useUnscaledTime: true)
                .Group(Tween.Color(spriteRenderer, targetColor, transitionDuration, transitionEase));

            if (newState == SpriteState.Staggered)
                currentSequence.Group(Tween.MaterialColor(material, materialColorId, materialStaggerTargetColor, transitionDuration, transitionEase));

            if (previousState == SpriteState.Staggered)
                currentSequence.Group(Tween.MaterialColor(material, materialColorId, startingMaterialColor, transitionDuration, transitionEase));

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
            if (Time.time - playerHealth.lastHitTimestamp <= playerData.staggerDuration)
                return SpriteState.Staggered;

            if (playerStamina.IsEmpty)
                return SpriteState.Exhausted;

            return SpriteState.Initial;
        }
    }
}
