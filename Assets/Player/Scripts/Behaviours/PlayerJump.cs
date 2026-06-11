using System;
using Player.Scripts;
using Tools_and_Scripts;
using UnityEngine;
using UnityEngine.Events;

public class PlayerJump : IPlayerBehaviour
{
    public UnityEvent OnStartJump = new UnityEvent();
    public UnityEvent OnLandJump = new UnityEvent();
    public UnityEvent OnStopJump = new UnityEvent();

    private float jumpStartTimestamp;
    private float jumpCooldownTimestamp = -1.0f;

    public bool hasLanded { get; private set; }

    public void StartBehaviour(PlayerStateMachine player, BehaviourType previous)
    {

        hasLanded = false;

        Vector2 inputDirection = player.moveInput.magnitude >= 0.15f ? player.moveInput.normalized : player.LastLookDirection;
        player.SetLastLookDirection(inputDirection);
        jumpStartTimestamp = Time.time;

        player.playerStamina.ConsumeStamina(player.playerData.jumpStaminaCost);

        OnStartJump?.Invoke();
    }

    public void UpdateBehaviour(PlayerStateMachine player)
    {
        if (!hasLanded && player.playerTagSystem != null && player.playerTagSystem.CanTag && player.inputPackage.GetTag.WasPressedWithBuffer())
        {
            player.ChangeBehaviour(player.playerTag);
            return;
        }

        if (!hasLanded && Time.time - jumpStartTimestamp >= player.playerData.jumpInAirDuration)
        {
            Land(player);
            return;
        }

        if (Time.time - jumpStartTimestamp >= player.playerData.jumpMaxDuration)
        {
            StopJump(player);
            return;
        }
    }

    private void Land(PlayerStateMachine player)
    {
        hasLanded = true;
        OnLandJump?.Invoke();
    }

    private void StopJump(PlayerStateMachine player)
    {
        if (player.moveInput.magnitude >= 0.15f)
            player.ChangeBehaviour(player.playerRun);
        else
            player.ChangeBehaviour(player.playerIdle);
    }

    public void FixedUpdateBehaviour(PlayerStateMachine player)
    {
        HandleAcceleration(player);
        player.ApplyMovement();
    }

    private void HandleAcceleration(PlayerStateMachine player)
    {
        Vector3 move = player.moveInput;
        float speed = player.playerData.jumpMaxSpeed;
        move *= speed;

        if (player.moveInput.magnitude <= 0.05f || hasLanded)
        {
            player.moveVelocity.x = Mathf.MoveTowards(player.moveVelocity.x, 0.0f, player.playerData.jumpDeceleration * Time.fixedDeltaTime);
            player.moveVelocity.z = Mathf.MoveTowards(player.moveVelocity.z, 0.0f, player.playerData.jumpDeceleration * Time.fixedDeltaTime);
        }
        else
        {
            player.moveVelocity.x = Mathf.MoveTowards(player.moveVelocity.x, move.x, player.playerData.jumpAcceleration * Time.fixedDeltaTime);
            player.moveVelocity.z = Mathf.MoveTowards(player.moveVelocity.z, move.y, player.playerData.jumpAcceleration * Time.fixedDeltaTime);
        }
    }

    public bool CanJump(PlayerStateMachine player)
    {
        return (jumpCooldownTimestamp < 0.0f || Time.time >= jumpCooldownTimestamp) && !player.playerStamina.IsEmpty;
    }

    public void StopBehaviour(PlayerStateMachine player, BehaviourType next)
    {
        jumpCooldownTimestamp = Time.time + player.playerData.jumpCooldown;
        player.moveVelocity = Vector3.ClampMagnitude(player.moveVelocity, player.playerData.walkMaxSpeed);
        OnStopJump?.Invoke();
    }

    public BehaviourType GetBehaviourType()
    {
        return BehaviourType.Jump;
    }
}
