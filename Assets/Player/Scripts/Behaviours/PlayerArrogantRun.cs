using Player.Scripts;
using UnityEngine;
using UnityEngine.Events;

public class PlayerArrogantRun : IPlayerBehaviour
{
    public UnityEvent OnStartBeingArrogant = new UnityEvent();
    public UnityEvent OnStopBeingArrogant = new UnityEvent();

    private bool isSkippingFrame;
    public bool IsSkippingFrame => isSkippingFrame;

    public void StartBehaviour(PlayerStateMachine player, BehaviourType previous)
    {
        if (previous == BehaviourType.Locked)
        {
            isSkippingFrame = true;
            player.inputPacker.ResetBuffers();
        }

        if (previous != BehaviourType.ArrogantIdle && previous != BehaviourType.ArrogantSpin)
            OnStartBeingArrogant?.Invoke();
    }

    public void UpdateBehaviour(PlayerStateMachine player)
    {
        if (isSkippingFrame)
        {
            isSkippingFrame = false;
            return;
        }

        if (player.playerArrogantSpin.CanSpin(player) && player.inputPackage.GetRoll.WasPressedWithBuffer())
        {
            player.ChangeBehaviour(player.playerArrogantSpin);
            return;
        }

        if (player.playerJump.CanJump(player) && player.inputPackage.GetJump.WasPressedWithBuffer())
        {
            player.ChangeBehaviour(player.playerJump);
            return;
        }

        if (player.playerAttack.CanAttack(player) && player.inputPackage.GetAttack.WasPressedWithBuffer())
        {
            player.ChangeBehaviour(player.playerAttack);
            return;
        }

        if (player.playerParry.CanParry(player) && player.inputPackage.GetParry.WasPressedWithBuffer())
        {
            player.ChangeBehaviour(player.playerParry);
            return;
        }

        if (!player.inputPackage.GetArroganceMode.isPressed)
        {
            player.ChangeBehaviour(player.playerRun);
            return;
        }

        if (player.moveInput.magnitude < 0.15f)
        {
            player.ChangeBehaviour(player.inputPackage.GetArroganceMode.isPressed ? player.playerArrogantIdle : player.playerIdle);
            return;
        }

        player.CheckForInteraction();
        player.ComputeLastLookDirection();
    }

    public void FixedUpdateBehaviour(PlayerStateMachine player)
    {
        if (CanPlayerControlDirection(player))
            HandleDirection(player);

        player.ApplyMovement();
    }

    private bool CanPlayerControlDirection(PlayerStateMachine player)
    {
        return !player.isLocked || player.playerLocked.GetLockState == PlayerLocked.LockState.Dialog;
    }

    private void HandleDirection(PlayerStateMachine player)
    {
        Vector3 move = player.moveInput;
        float speed = ComputeMoveSpeed(player);
        move *= speed;

        if (player.moveInput.magnitude <= 0.05f)
        {
            player.moveVelocity.x = Mathf.MoveTowards(player.moveVelocity.x, 0.0f, player.playerData.groundDeceleration * Time.fixedDeltaTime);
            player.moveVelocity.z = Mathf.MoveTowards(player.moveVelocity.z, 0.0f, player.playerData.groundDeceleration * Time.fixedDeltaTime);
        }
        else
        {
            player.moveVelocity.x = Mathf.MoveTowards(player.moveVelocity.x, move.x, player.playerData.groundAcceleration * Time.fixedDeltaTime);
            player.moveVelocity.z = Mathf.MoveTowards(player.moveVelocity.z, move.y, player.playerData.groundAcceleration * Time.fixedDeltaTime);
        }
    }

    private float ComputeMoveSpeed(PlayerStateMachine player)
    {
        if (player.isLocked)
        {

            if (player.playerLocked.GetLockState == PlayerLocked.LockState.Dialog)
                return player.playerData.dialogWalkMaxSpeed;
            else
                return 0.0f;
        }

        return player.playerData.arrogantWalkMaxSpeed;
    }

    public void StopBehaviour(PlayerStateMachine player, BehaviourType next)
    {
        if (next != BehaviourType.ArrogantIdle && next != BehaviourType.ArrogantSpin)
            OnStopBeingArrogant?.Invoke();
    }

    public BehaviourType GetBehaviourType()
    {
        return BehaviourType.ArrogantRun;
    }
}
