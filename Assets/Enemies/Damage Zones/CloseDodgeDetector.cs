using Player.Scripts;
using UnityEngine;

public class CloseDodgeDetector
{
    private float damageTimestamp;
    private float windowStartTimestamp;
    private float baseAmount;
    private Object source;

    private bool wasInsideZone;
    private bool wasInsideZoneOnPreviousUpdate;
    private bool wasArroganceModeActiveOnExit;
    private bool isActive;
    private bool isResolved;

    public void Setup(float damageTimestamp, float windowDuration, float baseAmount, Object source = null)
    {
        if (windowDuration <= 0.0f)
            return;

        this.damageTimestamp = damageTimestamp;
        windowStartTimestamp = Mathf.Max(Time.time, damageTimestamp - windowDuration);
        this.baseAmount = baseAmount;
        this.source = source;
        isActive = true;
    }

    public void Update(bool isPlayerInsideZone, bool isArroganceModeActive)
    {
        if (!isActive || isResolved)
            return;

        if (Time.time < windowStartTimestamp || Time.time >= damageTimestamp)
            return;

        TrackPlayerPresence(isPlayerInsideZone, isArroganceModeActive);
    }

    public void Resolve(bool isPlayerInsideZone, bool isArroganceModeActive)
    {
        if (!isActive || isResolved)
            return;

        if (Time.time >= windowStartTimestamp)
            TrackPlayerPresence(isPlayerInsideZone, isArroganceModeActive);

        isResolved = true;

        if (!wasInsideZone || isPlayerInsideZone)
            return;

        ArroganceGainEvents.RequestGain(new ArroganceGainRequest(
            baseAmount,
            ArroganceGainReason.CloseDodge,
            source,
            new CloseDodgeGainContext(wasArroganceModeActiveOnExit)));
    }

    private void TrackPlayerPresence(bool isPlayerInsideZone, bool isArroganceModeActive)
    {
        if (wasInsideZoneOnPreviousUpdate && !isPlayerInsideZone)
            wasArroganceModeActiveOnExit = isArroganceModeActive;

        wasInsideZone |= isPlayerInsideZone;
        wasInsideZoneOnPreviousUpdate = isPlayerInsideZone;
    }

    public void Cancel()
    {
        isResolved = true;
    }
}
