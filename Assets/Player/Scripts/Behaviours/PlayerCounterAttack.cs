using System;
using Player.Scripts;
using UnityEngine;

public class PlayerCounterAttack : IPlayerBehaviour
{
    private float attackStartTimestamp;

    private bool canAttackBeCanceled;
    private bool hasSpawnedDamageBox;
    private bool hasRemovedDamageBox;

    private AttackPayload currentAttackPayload;

    public void StartBehaviour(PlayerStateMachine player, BehaviourType previous)
    {
        attackStartTimestamp = Time.time;
        canAttackBeCanceled = false;
        hasSpawnedDamageBox = false;
        hasRemovedDamageBox = false;

        currentAttackPayload = new AttackPayload("CounterAttack", AttackType.Punish, player.playerData.counterAttackDamage, 1);

        player.playerAttack.OnPlayerAttack?.Invoke(currentAttackPayload);
    }

    public void UpdateBehaviour(PlayerStateMachine player)
    {
        if (Time.time - attackStartTimestamp >= player.playerData.counterAttackDuration)
        {
            if (player.inputPackage.GetArroganceMode.isPressed)
                player.ChangeBehaviour(player.playerArrogantIdle);
            else
                player.ChangeBehaviour(player.playerIdle);
            return;
        }

        if (!canAttackBeCanceled && Time.time - attackStartTimestamp >= player.playerData.counterAttackCancelTimer)
        {
            canAttackBeCanceled = true;
        }

        if (!hasSpawnedDamageBox && Time.time - attackStartTimestamp >= player.playerData.counterAttackSpawnHitBoxTimer)
        {
            hasSpawnedDamageBox = true;
            player.playerAttack.OnSpawnDamageBox?.Invoke();
        }

        if (!hasRemovedDamageBox && Time.time - attackStartTimestamp >= player.playerData.counterAttackRemoveHitBoxTimer)
        {
            hasRemovedDamageBox = true;
            player.playerAttack.OnRemoveDamageBox?.Invoke();
        }

        if (canAttackBeCanceled && player.TryStartCriticalAttack())
        {
            return;
        }

        if (canAttackBeCanceled && player.playerRoll.CanRoll(player) && player.inputPackage.GetRoll.WasPressedWithBuffer())
        {
            if (player.inputPackage.GetArroganceMode.isPressed)
                player.ChangeBehaviour(player.playerArrogantSpin);
            else
                player.ChangeBehaviour(player.playerRoll);
            return;
        }

        if (canAttackBeCanceled && player.playerJump.CanJump(player) && player.inputPackage.GetJump.WasPressedWithBuffer())
        {
            player.ChangeBehaviour(player.playerJump);
            return;
        }

        if (canAttackBeCanceled && player.playerParry.CanParry(player) && player.inputPackage.GetParry.WasPressedWithBuffer())
        {
            player.ChangeBehaviour(player.playerParry);
            return;
        }
    }

    public void FixedUpdateBehaviour(PlayerStateMachine player)
    {
        HandleDirection(player);

        player.ApplyMovement();
    }

    private void HandleDirection(PlayerStateMachine player)
    {
        Vector3 move = player.moveInput;
        float speed = player.playerData.counterAttackMoveSpeed;
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

    public void StopBehaviour(PlayerStateMachine player, BehaviourType next)
    {
        player.playerSword.RemoveHitbox();
    }

    public BehaviourType GetBehaviourType()
    {
        return BehaviourType.CounterAttack;
    }
}
