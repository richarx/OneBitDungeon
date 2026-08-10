using Player.Scripts;
using Tools_and_Scripts;
using UnityEngine;
using UnityEngine.Events;

public class PlayerArrogantSpin : IPlayerBehaviour
{
    public UnityEvent OnStartSpin = new UnityEvent();
    public UnityEvent OnStopSpin = new UnityEvent();

    private Vector3 spinDirection;
    private Vector3 spinStartPosition;
    private float spinStartTimestamp;
    private float spinCooldownTimestamp = -1.0f;

    public bool IsRollingLeft => spinDirection.x >= 0.0f;
    public bool IsSpinningClockwise;

    public void StartBehaviour(PlayerStateMachine player, BehaviourType previous)
    {

        Vector2 inputDirection = player.moveInput.magnitude >= 0.15f ? player.moveInput.normalized : player.LastLookDirection * -1.0f;
        //player.SetLastLookDirection(inputDirection);
        spinDirection = inputDirection.ToVector3();
        spinStartPosition = player.position;
        spinStartTimestamp = Time.time;

        float angle = Vector3.SignedAngle(player.LastLookDirection.ToVector3(), spinDirection, Vector3.up);
        IsSpinningClockwise = angle <= 0.0f;
        IsSpinningClockwise = player.LastLookDirection.y <= 0 ? !IsSpinningClockwise : IsSpinningClockwise;

        Debug.Log($"Angle : {angle} / {IsSpinningClockwise} / {player.LastLookDirection.y}");

        //player.playerStamina.ConsumeStamina(player.playerData.rollStaminaCost);

        OnStartSpin?.Invoke();
    }

    public void UpdateBehaviour(PlayerStateMachine player)
    {
        if (Time.time - spinStartTimestamp >= player.playerData.spinMaxDuration)
        {
            StopSpin(player);
            return;
        }

        if (Vector3.Distance(spinStartPosition, player.position) >= player.playerData.spinMaxDistance)
        {
            StopSpin(player);
            return;
        }
    }

    private void StopSpin(PlayerStateMachine player)
    {
        if (player.moveInput.magnitude >= 0.15f)
            player.ChangeBehaviour(player.inputPackage.GetArroganceMode.isPressed ? player.playerArrogantRun : player.playerRun);
        else
            player.ChangeBehaviour(player.inputPackage.GetArroganceMode.isPressed ? player.playerArrogantIdle : player.playerIdle);
    }

    public void FixedUpdateBehaviour(PlayerStateMachine player)
    {
        HandleAcceleration(player);
        player.ApplyMovement();
    }

    private void HandleAcceleration(PlayerStateMachine player)
    {
        Vector3 move = spinDirection * player.playerData.spinMaxSpeed;
        float distanceMoved = Vector3.Distance(spinStartPosition, player.position);
        float normalizedDistance = Tools.NormalizeValue(distanceMoved, 0.0f, player.playerData.spinMaxDistance);

        if (normalizedDistance >= player.playerData.spinDecelerationDistanceThreshold)
        {
            player.moveVelocity.x = Mathf.MoveTowards(player.moveVelocity.x, 0.0f, player.playerData.spinDeceleration * Time.fixedDeltaTime);
            player.moveVelocity.z = Mathf.MoveTowards(player.moveVelocity.z, 0.0f, player.playerData.spinDeceleration * Time.fixedDeltaTime);
        }
        else
        {
            player.moveVelocity.x = Mathf.MoveTowards(player.moveVelocity.x, move.x, player.playerData.spinAcceleration * Time.fixedDeltaTime);
            player.moveVelocity.z = Mathf.MoveTowards(player.moveVelocity.z, move.z, player.playerData.spinAcceleration * Time.fixedDeltaTime);
        }
    }

    public bool CanSpin(PlayerStateMachine player)
    {
        return (spinCooldownTimestamp < 0.0f || Time.time >= spinCooldownTimestamp) && !player.playerStamina.IsEmpty;
    }

    public void StopBehaviour(PlayerStateMachine player, BehaviourType next)
    {
        spinCooldownTimestamp = Time.time + player.playerData.spinCooldown;
        player.moveVelocity = Vector3.ClampMagnitude(player.moveVelocity, player.inputPackage.GetArroganceMode.isPressed ? player.playerData.arrogantWalkMaxSpeed : player.playerData.walkMaxSpeed);
        OnStopSpin?.Invoke();
    }

    public BehaviourType GetBehaviourType()
    {
        return BehaviourType.ArrogantSpin;
    }
}
