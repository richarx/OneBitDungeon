using Sirenix.OdinInspector;
using Tools_and_Scripts;
using UnityEngine;

namespace Player.Scripts
{
    public class PlayerArrogance : MonoBehaviour
    {
        [TitleGroup("Insolence"), ShowInInspector, ReadOnly, LabelText("Current Arrogance")]
        private float InspectorCurrentArrogance => currentArrogance;

        [TitleGroup("Insolence"), ShowInInspector, ReadOnly, ProgressBar(0.0f, 1.0f), LabelText("Arrogance Fill")]
        private float InspectorNormalizedArrogance => NormalizedArrogance;

        private PlayerData playerData;
        private float currentArrogance;

        public float CurrentArrogance => currentArrogance;
        public float NormalizedArrogance => playerData == null || playerData.maxArrogance <= 0.0f
            ? 0.0f
            : currentArrogance / playerData.maxArrogance;

        private void OnEnable()
        {
            ArroganceGainEvents.OnGainRequested += HandleGainRequest;
        }

        private void OnDisable()
        {
            ArroganceGainEvents.OnGainRequested -= HandleGainRequest;
        }

        private void Start()
        {
            playerData = PlayerStateMachine.instance.playerData;
            currentArrogance = Mathf.Clamp(currentArrogance, 0.0f, playerData.maxArrogance);
        }

        public void UpdateArrogance(InputPackage inputPackage)
        {
            if (inputPackage.GetClearArrogance.wasPressedThisFrame)
                ClearArrogance();
        }

        private void HandleGainRequest(ArroganceGainRequest request)
        {
            if (request == null)
                return;

            GainArrogance(ComputeFinalGainAmount(request));
        }

        private float ComputeFinalGainAmount(ArroganceGainRequest request)
        {
            float amount = request.baseAmount;
            CloseDodgeGainContext closeDodgeContext = request.context as CloseDodgeGainContext;

            if (closeDodgeContext != null && closeDodgeContext.wasArroganceModeActiveOnExit)
                amount *= playerData.arroganceStateGainMultiplier;

            return Mathf.Max(0.0f, amount);
        }

        private void GainArrogance(float amount)
        {
            if (playerData == null)
                return;

            currentArrogance = Mathf.Clamp(currentArrogance + amount, 0.0f, playerData.maxArrogance);
        }

        public void ClearArrogance()
        {
            currentArrogance = 0.0f;
        }

        [TitleGroup("Debug"), Button("Gain Debug Arrogance"), EnableIf(nameof(IsPlaying))]
        private void TestGainArrogance()
        {
            if (playerData == null)
                return;

            float timestamp = Time.time;
            ArroganceGainEvents.RequestGain(new ArroganceGainRequest(
                playerData.arroganceGainOnCloseDodge,
                ArroganceGainReason.Debug,
                this));
        }

        [TitleGroup("Debug"), Button("Clear Arrogance"), EnableIf(nameof(IsPlaying))]
        private void TestClearArrogance()
        {
            ClearArrogance();
        }

        private bool IsPlaying => Application.isPlaying;
    }
}
