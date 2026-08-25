using Player.Scripts;
using UnityEngine;

public class PlayerJumpAttack : IPlayerBehaviour
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

        currentAttackPayload = new AttackPayload("JumpAttack", AttackType.Jump, player.playerData.normalAttackDamage, 1);

        player.playerStamina.ConsumeStamina(0);

        player.playerAttack.OnPlayerAttack?.Invoke(currentAttackPayload);
    }

    public void UpdateBehaviour(PlayerStateMachine player)
    {
        if (Time.time - attackStartTimestamp >= player.playerData.jumpAttackDuration)
        {
            if (player.inputPackage.GetArroganceMode.isPressed)
                player.ChangeBehaviour(player.playerArrogantIdle);
            else
                player.ChangeBehaviour(player.playerIdle);
            return;
        }

        if (!canAttackBeCanceled && Time.time - attackStartTimestamp >= player.playerData.jumpAttackCancelTimer)
        {
            canAttackBeCanceled = true;
        }

        if (!hasSpawnedDamageBox && Time.time - attackStartTimestamp >= player.playerData.jumpAttackSpawnHitBoxTimer)
        {
            hasSpawnedDamageBox = true;
            player.playerAttack.OnSpawnDamageBox?.Invoke();
        }

        if (!hasRemovedDamageBox && Time.time - attackStartTimestamp >= player.playerData.jumpAttackRemoveHitBoxTimer)
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
        player.moveVelocity = Vector3.zero;
        player.ApplyMovement();
    }

    public void StopBehaviour(PlayerStateMachine player, BehaviourType next)
    {
        player.playerSword.RemoveHitbox();
    }

    public BehaviourType GetBehaviourType()
    {
        return BehaviourType.JumpAttack;
    }
}
