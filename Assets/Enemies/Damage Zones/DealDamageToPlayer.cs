using Player.Scripts;
using UnityEngine;

public class DealDamageToPlayer : MonoBehaviour
{
    [SerializeField] private int damage;
    [SerializeField] private bool canBeParried;
    [SerializeField] private bool canBeJumped;

    private bool hasBeenParried = false;

    public bool TryDealDamage(Vector3 direction)
    {
        if (hasBeenParried)
            return false;

        PlayerStateMachine player = PlayerStateMachine.instance;
        BehaviourType currentBehaviour = player.currentBehaviour.GetBehaviourType();

        if (canBeJumped && currentBehaviour == BehaviourType.Jump && !player.playerJump.hasLanded)
            return false;

        if (canBeParried && player.playerHealth.IsParrying())
        {
            player.playerHealth.TriggerParry();
            hasBeenParried = true;
            return false;
        }

        bool isDamageApplied = player.playerHealth.TakeDamage(damage, direction.normalized);

        return isDamageApplied;
    }
}
