using Player.Scripts;
using System.Collections.Generic;
using UnityEngine;

public class CloseDodgeDetector
{
    private float damageTimestamp;
    private float windowStartTimestamp;
    private float baseAmount;
    private Object source;
    private CloseDodgeSession session;

    private bool wasInsideZone;
    private bool wasInsideZoneOnPreviousUpdate = false;
    private bool wasArroganceModeActiveOnExit;
    private float normalizedExitTime;
    private bool isActive;
    private bool isResolved;

    private PlayerStateMachine player;
    private bool isPlayerSpinning => player.currentBehaviour.GetBehaviourType() == BehaviourType.ArrogantSpin;

    public void Setup(float damageTimestamp, float windowDuration, float baseAmount, Object source = null, CloseDodgeSession session = null)
    {
        if (windowDuration <= 0.0f)
            return;

        player = PlayerStateMachine.instance;

        this.damageTimestamp = damageTimestamp;
        windowStartTimestamp = damageTimestamp - windowDuration;
        this.baseAmount = baseAmount;
        wasInsideZone = false;
        wasArroganceModeActiveOnExit = false;
        wasInsideZone = false;
        this.source = source;
        this.session = session;
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

        if (!wasInsideZone || isPlayerInsideZone || !isPlayerSpinning)
            return;

        ArroganceGainRequest gain = new ArroganceGainRequest(
            baseAmount,
            ArroganceGainReason.CloseDodge,
            source,
            new CloseDodgeGainContext(wasArroganceModeActiveOnExit, normalizedExitTime));

        if (session != null)
            session.RegisterDodge(gain);
        else
            ArroganceGainEvents.RequestGain(gain);
    }

    private void TrackPlayerPresence(bool isPlayerInsideZone, bool isArroganceModeActive)
    {
        if (wasInsideZoneOnPreviousUpdate && !isPlayerInsideZone)
        {
            wasArroganceModeActiveOnExit = isArroganceModeActive;
            normalizedExitTime = Mathf.InverseLerp(windowStartTimestamp, damageTimestamp, Time.time);
        }

        wasInsideZone |= isPlayerInsideZone;
        wasInsideZoneOnPreviousUpdate = isPlayerInsideZone;
    }

    public void Cancel()
    {
        isResolved = true;
    }
}

/// <summary>
/// Delays close-dodge rewards until every damage zone in one attack salvo has
/// finished its damage check. A hit from any zone invalidates the whole salvo.
/// </summary>
public enum CloseDodgeSessionOutcome
{
    Hit,
    CloseDodge,
    NoCloseDodge
}

public class CloseDodgeSession
{
    private readonly int expectedDamageChecks;
    private readonly List<ArroganceGainRequest> pendingGains = new List<ArroganceGainRequest>();

    private int completedDamageChecks;
    private bool playerWasHit;
    private bool isCompleted;

    public event System.Action<CloseDodgeSessionOutcome> OnCompleted;

    public CloseDodgeSession(int expectedDamageChecks)
    {
        this.expectedDamageChecks = expectedDamageChecks;
    }

    public void RegisterDodge(ArroganceGainRequest gain)
    {
        if (!isCompleted)
            pendingGains.Add(gain);
    }

    public void RegisterHit()
    {
        playerWasHit = true;
    }

    public void CompleteDamageCheck()
    {
        if (isCompleted)
            return;

        completedDamageChecks++;

        if (completedDamageChecks < expectedDamageChecks)
            return;

        isCompleted = true;

        CloseDodgeSessionOutcome outcome = playerWasHit
            ? CloseDodgeSessionOutcome.Hit
            : pendingGains.Count > 0
                ? CloseDodgeSessionOutcome.CloseDodge
                : CloseDodgeSessionOutcome.NoCloseDodge;

        if (!playerWasHit)
        {
            // Version get all the gains from the pending list and request them at once.
            // foreach (ArroganceGainRequest gain in pendingGains)
            //     ArroganceGainEvents.RequestGain(gain);

            // Version get Max gain from the pending list and request it.
            if (pendingGains.Count > 0)
            {
                ArroganceGainRequest maxGain = pendingGains[0];
                foreach (ArroganceGainRequest gain in pendingGains)
                {
                    if (gain.baseAmount > maxGain.baseAmount)
                        maxGain = gain;
                }
                ArroganceGainEvents.RequestGain(maxGain);
            }
        }

        pendingGains.Clear();
        OnCompleted?.Invoke(outcome);
    }

    public void Cancel()
    {
        isCompleted = true;
        pendingGains.Clear();
    }
}
