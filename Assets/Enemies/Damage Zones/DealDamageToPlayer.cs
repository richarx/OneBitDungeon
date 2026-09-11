using System;
using Player.Scripts;
using UnityEngine;

public class DealDamageToPlayer : MonoBehaviour
{
    [SerializeField] private int damage;
    [SerializeField] private bool canBeParried;
    [SerializeField] private bool canBeJumped;

    private bool hasBeenParried = false;
    private bool _hasPublishedJumpAvoidedAttack;

    public event Action OnPlayerJumpAvoidedAttack;

    public void Configure(int newDamage, bool parryable, bool jumpable)
    {
        damage = Mathf.Max(0, newDamage);
        canBeParried = parryable;
        canBeJumped = jumpable;
        hasBeenParried = false;
        _hasPublishedJumpAvoidedAttack = false;
    }

    public bool TryDealDamage(Vector3 direction, float staggerPower = -1.0f)
    {
        if (hasBeenParried)
            return false;

        PlayerStateMachine player = PlayerStateMachine.instance;
        if (player == null)
            return false;

        BehaviourType currentBehaviour = player.currentBehaviour.GetBehaviourType();

        if (canBeJumped && ((currentBehaviour == BehaviourType.Jump && !player.playerJump.hasLanded) || currentBehaviour == BehaviourType.JumpTag))
        {
            if (!_hasPublishedJumpAvoidedAttack)
            {
                _hasPublishedJumpAvoidedAttack = true;
                OnPlayerJumpAvoidedAttack?.Invoke();
            }

            return false;
        }

        if (canBeParried && player.playerHealth.IsParrying())
        {
            player.playerHealth.TriggerParry();
            hasBeenParried = true;
            return false;
        }

        bool isDamageApplied = player.playerHealth.TakeDamage(damage, direction.normalized, staggerPower);

        return isDamageApplied;
    }
}
