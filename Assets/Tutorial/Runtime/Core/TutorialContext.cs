using Player.Scripts;

namespace Tutorials
{
    public sealed class TutorialContext
    {
        public TutorialContext(PlayerStateMachine player)
        {
            Player = player;
        }

        public PlayerStateMachine Player { get; }
    }
}
