namespace Player.Scripts
{
    public class CloseDodgeGainContext : IArroganceGainContext
    {
        public readonly bool wasArroganceModeActiveOnExit;

        public CloseDodgeGainContext(bool wasArroganceModeActiveOnExit)
        {
            this.wasArroganceModeActiveOnExit = wasArroganceModeActiveOnExit;
        }
    }
}
