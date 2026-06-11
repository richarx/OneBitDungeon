using UnityEngine;

namespace Player.Scripts
{
    public enum TagContext
    {
        None,
        Attack,
        Roll,
        Jump,
        SucceededParry
    }


    public class PlayerTag : IPlayerBehaviour
    {
        private float tagStartTimestamp;

        public TagContext TagContext { get; private set; } = TagContext.None;

        public void StartBehaviour(PlayerStateMachine player, BehaviourType previous)
        {
            tagStartTimestamp = Time.time;
            player.playerTagSystem.Swap();

            // We handle here the behaviour if last behaviour was an attack, a parry or a roll since the follow up is basically an attack with effect
            // Jump has its own follow up behaviour so it got its own behaviour PlayerJumpTag

            if (previous is BehaviourType.Attack)
            {
                TagContext = TagContext.Attack;
                player.ChangeBehaviour(player.playerAttack);
            }
            else if (previous is BehaviourType.Roll)
            {
                TagContext = TagContext.Roll;
                player.ChangeBehaviour(player.playerAttack);
            }
            else if (previous is BehaviourType.Jump)
            {
                TagContext = TagContext.Jump;
                player.ChangeBehaviour(player.playerJumpTag);
            }
            else if (previous is BehaviourType.Parry)
            {
                TagContext = TagContext.SucceededParry;
                player.ChangeBehaviour(player.playerAttack);
            }
            else
            {
                TagContext = TagContext.None;
                player.ChangeBehaviour(player.playerIdle);
            }

        }

        public void UpdateBehaviour(PlayerStateMachine player)
        {
            if (Time.time - tagStartTimestamp >= player.playerData.tagDuration)
                player.ChangeBehaviour(player.playerIdle);
        }

        public void FixedUpdateBehaviour(PlayerStateMachine player)
        {
        }

        public void StopBehaviour(PlayerStateMachine player, BehaviourType next) { }

        public BehaviourType GetBehaviourType() => BehaviourType.Tag;
    }
}
