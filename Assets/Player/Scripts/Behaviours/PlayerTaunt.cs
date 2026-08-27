using System.Collections;
using System.Collections.Generic;
using Player.Scripts;
using UnityEngine;
using UnityEngine.Events;

public class PlayerTaunt : IPlayerBehaviour
{
    public UnityEvent OnStartTaunt = new UnityEvent();
    public UnityEvent OnStopTaunt = new UnityEvent();

    private float lastArroganceGainTimestamp = -1.0f;

    public void StartBehaviour(PlayerStateMachine player, BehaviourType previous)
    {
        lastArroganceGainTimestamp = Time.time;

        OnStartTaunt?.Invoke();
    }

    public void UpdateBehaviour(PlayerStateMachine player)
    {
        if (player.inputPackage.GetArroganceMode.isPressed)
        {
            player.ChangeBehaviour(player.playerArrogantIdle);
            return;
        }

        if (player.playerRoll.CanRoll(player) && player.inputPackage.GetRoll.WasPressedWithBuffer())
        {
            player.ChangeBehaviour(player.playerRoll);
            return;
        }

        if (player.TryStartCriticalAttack())
        {
            return;
        }

        if (player.TryStartAttack())
        {
            return;
        }

        if (player.playerParry.CanParry(player) && player.inputPackage.GetParry.WasPressedWithBuffer())
        {
            player.ChangeBehaviour(player.playerParry);
            return;
        }

        if (player.playerJump.CanJump(player) && player.inputPackage.GetJump.WasPressedWithBuffer())
        {
            player.ChangeBehaviour(player.playerJump);
            return;
        }

        if (player.moveInput.magnitude >= 0.15f)
        {
            player.ChangeBehaviour(player.playerRun);
            return;
        }

        if (player.inputPackage.GetSitDown.wasPressedThisFrame)
        {
            player.ChangeBehaviour(player.playerSit);
            return;
        }

        if (player.playerTargeting.hasTarget)
            ComputeArroganceGain(player);
    }

    private void ComputeArroganceGain(PlayerStateMachine player)
    {
        if (Time.time - lastArroganceGainTimestamp >= 0.5f)
        {
            ArroganceGainEvents.RequestGain(new ArroganceGainRequest(player.playerData.arroganceGainWhileSitting, ArroganceGainReason.Taunt));
            lastArroganceGainTimestamp = Time.time;
        }
    }

    public void FixedUpdateBehaviour(PlayerStateMachine player)
    {
        player.moveVelocity = Vector3.zero;
        player.ApplyMovement();
    }

    public void StopBehaviour(PlayerStateMachine player, BehaviourType next)
    {
        OnStopTaunt?.Invoke();
    }

    public BehaviourType GetBehaviourType()
    {
        return BehaviourType.Taunt;
    }
}
