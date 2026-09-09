using UnityEngine;

namespace Player.Scripts
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerStateMachine))]
    public class ArroganceProcessor : MonoBehaviour
    {
        private PlayerData playerData;

        public float TauntGainMultiplier { get; set; } = 1.0f;

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

            if (request.reason == ArroganceGainReason.Taunt)
            {
                amount = ApplyTauntDangerZoneModifier(amount);
                //Debug.Log($"[ArroganceProcessor] Applying TauntDangerZoneModifier: {amount} (TauntGainMultiplier: {TauntGainMultiplier})");
                amount *= TauntGainMultiplier;
            }
            else
            {
                amount = ApplyProgressiveCloseDodgeGain(amount, request);
                amount = ApplyArroganceModeModifier(amount, request);
            }

            return Mathf.Max(0.0f, amount);
        }

        private float ApplyTauntDangerZoneModifier(float amount)
        {
            CloseDodgeDangerSnapshot dangerSnapshot = CloseDodgeDetector.GetPlayerDangerSnapshot();

            if (!dangerSnapshot.isPlayerInsideDangerZone)
                return amount;

            return EvaluateTauntDangerZoneGainPerSecond(dangerSnapshot.minimumSecondsUntilDamage) * Time.deltaTime;
        }

        private float EvaluateTauntDangerZoneGainPerSecond(float secondsUntilDamage)
        {
            if (!playerData.useProgressiveTauntDangerZoneGain)
                return playerData.tauntDangerZoneGainPerSecond;

            float progressionDuration = playerData.tauntDangerZoneProgressionDuration;
            if (progressionDuration <= 0.0f)
                return playerData.tauntDangerZoneMaximumGainPerSecond;

            float normalizedProgress = 1.0f - Mathf.Clamp01(secondsUntilDamage / progressionDuration);
            return Mathf.Lerp(
                playerData.tauntDangerZoneMinimumGainPerSecond,
                playerData.tauntDangerZoneMaximumGainPerSecond,
                normalizedProgress);
        }

        private float ApplyProgressiveCloseDodgeGain(float amount, ArroganceGainRequest request)
        {
            if (!playerData.useProgressiveArroganceGain)
                return amount;

            CloseDodgeGainContext closeDodgeContext = request.context as CloseDodgeGainContext;

            if (closeDodgeContext == null)
                return amount;

            return amount * EvaluateProgressiveCloseDodgeGain(closeDodgeContext.normalizedExitTime);
        }

        private float EvaluateProgressiveCloseDodgeGain(float normalizedExitTime)
        {
            float normalizedTime = Mathf.Clamp01(normalizedExitTime);

            switch (playerData.progressiveArroganceGainEasing)
            {
                case ArroganceGainEasing.QuadOut:
                    return 1.0f - Mathf.Pow(1.0f - normalizedTime, 2.0f);
                case ArroganceGainEasing.QuadIn:
                    return normalizedTime * normalizedTime;
                default:
                    return normalizedTime;
            }
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
