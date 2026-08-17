using UnityEngine;

namespace Player.Scripts
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerStateMachine))]
    public class ArroganceProcessor : MonoBehaviour
    {
        private PlayerData playerData;

        private void Awake()
        {
            playerData = GetComponent<PlayerStateMachine>().playerData;
        }

        private void OnEnable()
        {
            ArroganceGainEvents.OnGainRequested += HandleGainRequest;
        }

        private void OnDisable()
        {
            ArroganceGainEvents.OnGainRequested -= HandleGainRequest;
        }

        private void HandleGainRequest(ArroganceGainRequest request)
        {
            if (request == null)
                return;

            float totalAmount = ComputeFinalGainAmount(request);
            ArroganceGainEvents.PublishProcessedGain(new ArroganceGainResult(request, totalAmount));
        }

        private float ComputeFinalGainAmount(ArroganceGainRequest request)
        {
            float amount = request.baseAmount;

            amount = ApplyArroganceModeModifier(amount, request);

            return Mathf.Max(0.0f, amount);
        }

        private float ApplyArroganceModeModifier(float amount, ArroganceGainRequest request)
        {
            CloseDodgeGainContext closeDodgeContext = request.context as CloseDodgeGainContext;

            if (closeDodgeContext == null || !closeDodgeContext.wasArroganceModeActiveOnExit)
                return amount;

            return amount * playerData.arroganceStateGainMultiplier;
        }
    }
}
