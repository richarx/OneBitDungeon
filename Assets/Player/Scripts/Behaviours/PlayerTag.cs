using UnityEngine;

namespace Player.Scripts
{
    public enum TagContext
    {
        None,
        Attack,
        Roll,
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
                player.playerAttack.TriggerTagIn(player);
            }
            else if (previous is BehaviourType.Roll)
            {
                TagContext = TagContext.Roll;
            }
            else if (previous is BehaviourType.Parry)
            {
                TagContext = TagContext.SucceededParry;
            }
            else
            {
                TagContext = TagContext.None;
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
