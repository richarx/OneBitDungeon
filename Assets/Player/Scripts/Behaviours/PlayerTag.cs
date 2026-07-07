using UnityEngine;
using UnityEngine.Events;

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

        public UnityEvent<TagContext> OnPlayerTag = new UnityEvent<TagContext>();

        public void StartBehaviour(PlayerStateMachine player, BehaviourType previous)
        {
            tagStartTimestamp = Time.time;
            player.playerTagSystem.Swap();

            // We handle here the behaviour if last behaviour was an attack, a parry or a roll since the follow up is basically an attack with effect
            // Jump has its own follow up behaviour so it got its own behaviour PlayerJumpTag

            TagContext = ComputeTagContext(previous);
            OnPlayerTag?.Invoke(TagContext);

            switch (TagContext)
            {
                case TagContext.None:
                    player.ChangeBehaviour(player.playerIdle);
                    break;
                case TagContext.Attack:
                    player.ChangeBehaviour(player.playerAttack);
                    break;
                case TagContext.Roll:
                    player.ChangeBehaviour(player.playerAttack);
                    break;
                case TagContext.Jump:
                    player.ChangeBehaviour(player.playerJumpTag);
                    break;
                case TagContext.SucceededParry:
                    player.ChangeBehaviour(player.playerAttack);
                    break;
            }
        }

        private TagContext ComputeTagContext(BehaviourType previous)
        {
            switch (previous)
            {
                case BehaviourType.Attack:
                    return TagContext.Attack;
                case BehaviourType.Roll:
                    return TagContext.Roll;
                case BehaviourType.Jump:
                    return TagContext.Jump;
                case BehaviourType.Parry:
                    return TagContext.SucceededParry;
                case BehaviourType.Idle:
                case BehaviourType.Run:
                case BehaviourType.JumpTag:
                case BehaviourType.Stagger:
                case BehaviourType.Sit:
                case BehaviourType.Dead:
                case BehaviourType.Locked:
                case BehaviourType.Tag:
                default:
                    return TagContext.None;
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
