using Game_Manager;
using Sirenix.OdinInspector;
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
        public bool IsFull => playerData != null && playerData.maxArrogance > 0.0f
            && currentArrogance >= playerData.maxArrogance;
        public float NormalizedArrogance => playerData == null || playerData.maxArrogance <= 0.0f
            ? 0.0f
            : currentArrogance / playerData.maxArrogance;

        private void OnEnable()
        {
            ArroganceGainEvents.OnGainProcessed += HandleProcessedGain;
            GameManager.OnRestartLevel.AddListener(ClearArrogance);
        }

        private void OnDisable()
        {
            ArroganceGainEvents.OnGainProcessed -= HandleProcessedGain;
            GameManager.OnRestartLevel.RemoveListener(ClearArrogance);
        }

        private void Start()
        {
            playerData = PlayerStateMachine.instance.playerData;
            currentArrogance = Mathf.Clamp(currentArrogance, 0.0f, playerData.maxArrogance);
        }

        private void HandleProcessedGain(ArroganceGainResult result)
        {

            if (result == null)
                return;

            if (playerData == null)
                return;

            currentArrogance = Mathf.Clamp(currentArrogance + result.totalAmount, 0.0f, playerData.maxArrogance);
        }


        public void ClearArrogance()
        {
            currentArrogance = 0.0f;
        }

        public void FillArrogance()
        {
            if (playerData == null)
                return;

            currentArrogance = playerData.maxArrogance;
        }

        public void LoseArrogance(float amount)
        {
            currentArrogance = Mathf.Max(0.0f, currentArrogance - Mathf.Max(0.0f, amount));
        }

        public bool ConsumeFullArrogance()
        {
            if (!IsFull)
                return false;

            ClearArrogance();
            return true;
        }

        [TitleGroup("Debug"), Button("Gain Debug Arrogance"), EnableIf(nameof(IsPlaying))]
        private void TestGainArrogance()
        {
            if (playerData == null)
                return;

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
