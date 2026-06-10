using UnityEngine;

namespace Player.Scripts
{
    public class PlayerTag : IPlayerBehaviour
    {
        private float tagStartTimestamp;

        public void StartBehaviour(PlayerStateMachine player, BehaviourType previous)
        {
            tagStartTimestamp = Time.time;
            player.playerTagSystem.Swap();
            player.playerAttack.TriggerTagIn(player);
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
