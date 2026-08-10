using Player.Scripts;
using UnityEngine;

public class PlayerArrogantIdle : IPlayerBehaviour
{
    public void StartBehaviour(PlayerStateMachine player, BehaviourType previous)
    {
        //Debug.Log("ARROGANT IDLE");
    }

    public void UpdateBehaviour(PlayerStateMachine player)
    {
        if (player.playerArrogantSpin.CanSpin(player) && player.inputPackage.GetRoll.WasPressedWithBuffer())
        {
            player.ChangeBehaviour(player.playerArrogantSpin);
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

        if (player.playerJump.CanJump(player) && player.inputPackage.GetJump.WasPressedWithBuffer())
        {
            player.ChangeBehaviour(player.playerJump);
            return;
        }

        if (player.moveInput.magnitude >= 0.15f)
        {
            player.ChangeBehaviour(player.playerArrogantRun);
            return;
        }

        if (!player.inputPackage.GetArroganceMode.isPressed)
        {
            player.ChangeBehaviour(player.playerIdle);
        }

        player.CheckForInteraction();
        player.ComputeLastLookDirection();
    }

    public void FixedUpdateBehaviour(PlayerStateMachine player)
    {
        SlowDownPlayer(player);
        player.ApplyMovement();
    }

    private void SlowDownPlayer(PlayerStateMachine player)
    {
        player.moveVelocity.x = Mathf.MoveTowards(player.moveVelocity.x, 0.0f, player.playerData.groundDeceleration * Time.fixedDeltaTime);
        player.moveVelocity.z = Mathf.MoveTowards(player.moveVelocity.z, 0.0f, player.playerData.groundDeceleration * Time.fixedDeltaTime);
    }

    public void StopBehaviour(PlayerStateMachine player, BehaviourType next)
    {

    }

    public BehaviourType GetBehaviourType()
    {
        return BehaviourType.ArrogantIdle;
    }
}
