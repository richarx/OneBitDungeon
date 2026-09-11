using System;
using UnityEngine;

namespace Player.Scripts
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerStateMachine))]
    public class ArroganceProcessor : MonoBehaviour
    {
        private const float TauntEventDuration = 1.0f;

        private PlayerData _playerData;
        private float _tauntDuration;
        private float _dangerZoneTauntDuration;
        private int _lastTauntRequestFrame = -1;
        private bool _hasPublishedTauntEvent;
        private bool _hasPublishedDangerZoneTauntEvent;

        public float TauntGainMultiplier { get; set; } = 1.0f;
        public event Action OnTauntedForOneSecond;
        public event Action OnTauntedInDangerZoneForOneSecond;

        private void Awake()
        {
            _playerData = GetComponent<PlayerStateMachine>().playerData;
        }

        private void OnEnable()
        {
            ArroganceGainEvents.OnGainRequested += HandleGainRequest;
        }

        private void OnDisable()
        {
            ArroganceGainEvents.OnGainRequested -= HandleGainRequest;
            ResetTauntProgress();
        }

        private void LateUpdate()
        {
            if (_lastTauntRequestFrame != Time.frameCount)
                ResetTauntProgress();
        }

        private void HandleGainRequest(ArroganceGainRequest request)
        {
            if (request == null)
                return;

            bool isTaunt = request.reason == ArroganceGainReason.Taunt;
            CloseDodgeDangerSnapshot dangerSnapshot = isTaunt
                ? CloseDodgeDetector.GetPlayerDangerSnapshot()
                : default(CloseDodgeDangerSnapshot);
            float totalAmount = ComputeFinalGainAmount(request, dangerSnapshot);

            ArroganceGainEvents.PublishProcessedGain(new ArroganceGainResult(request, totalAmount));

            if (isTaunt)
                TrackTaunt(dangerSnapshot.isPlayerInsideDangerZone);
        }

        private float ComputeFinalGainAmount(ArroganceGainRequest request, CloseDodgeDangerSnapshot dangerSnapshot)
        {
            float amount = request.baseAmount;

            if (request.reason == ArroganceGainReason.Taunt)
            {
                amount = ApplyTauntDangerZoneModifier(amount, dangerSnapshot);
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

        private float ApplyTauntDangerZoneModifier(float amount, CloseDodgeDangerSnapshot dangerSnapshot)
        {
            if (!dangerSnapshot.isPlayerInsideDangerZone)
                return amount;

            return EvaluateTauntDangerZoneGainPerSecond(dangerSnapshot.minimumSecondsUntilDamage) * Time.deltaTime;
        }

        private float EvaluateTauntDangerZoneGainPerSecond(float secondsUntilDamage)
        {
            if (!_playerData.useProgressiveTauntDangerZoneGain)
                return _playerData.tauntDangerZoneGainPerSecond;

            float progressionDuration = _playerData.tauntDangerZoneProgressionDuration;
            if (progressionDuration <= 0.0f)
                return _playerData.tauntDangerZoneMaximumGainPerSecond;

            float normalizedProgress = 1.0f - Mathf.Clamp01(secondsUntilDamage / progressionDuration);
            return Mathf.Lerp(
                _playerData.tauntDangerZoneMinimumGainPerSecond,
                _playerData.tauntDangerZoneMaximumGainPerSecond,
                normalizedProgress);
        }

        private float ApplyProgressiveCloseDodgeGain(float amount, ArroganceGainRequest request)
        {
            if (!_playerData.useProgressiveArroganceGain)
                return amount;

            CloseDodgeGainContext closeDodgeContext = request.context as CloseDodgeGainContext;

            if (closeDodgeContext == null)
                return amount;

            return amount * EvaluateProgressiveCloseDodgeGain(closeDodgeContext.normalizedExitTime);
        }

        private float EvaluateProgressiveCloseDodgeGain(float normalizedExitTime)
        {
            float normalizedTime = Mathf.Clamp01(normalizedExitTime);

            switch (_playerData.progressiveArroganceGainEasing)
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

            return amount * _playerData.arroganceStateGainMultiplier;
        }

        private void TrackTaunt(bool isPlayerInsideDangerZone)
        {
            if (_lastTauntRequestFrame == Time.frameCount)
                return;

            _lastTauntRequestFrame = Time.frameCount;

            if (!_hasPublishedTauntEvent)
            {
                _tauntDuration += Time.deltaTime;
                if (_tauntDuration >= TauntEventDuration)
                {
                    _hasPublishedTauntEvent = true;
                    OnTauntedForOneSecond?.Invoke();
                }
            }

            if (!isPlayerInsideDangerZone)
            {
                ResetDangerZoneTauntProgress();
                return;
            }

            if (_hasPublishedDangerZoneTauntEvent)
                return;

            _dangerZoneTauntDuration += Time.deltaTime;
            if (_dangerZoneTauntDuration < TauntEventDuration)
                return;

            _hasPublishedDangerZoneTauntEvent = true;
            OnTauntedInDangerZoneForOneSecond?.Invoke();
        }

        private void ResetTauntProgress()
        {
            _tauntDuration = 0.0f;
            _lastTauntRequestFrame = -1;
            _hasPublishedTauntEvent = false;
            ResetDangerZoneTauntProgress();
        }

        /// <summary>
        /// Starts a fresh taunt measurement for an exercise. A held input from the
        /// previous objective must not count toward the next one.
        /// </summary>
        public void ResetTauntDurationTracking()
        {
            ResetTauntProgress();
        }

        private void ResetDangerZoneTauntProgress()
        {
            _dangerZoneTauntDuration = 0.0f;
            _hasPublishedDangerZoneTauntEvent = false;
        }
    }
}
