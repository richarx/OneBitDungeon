namespace Player.Scripts
{
    public class CloseDodgeGainContext : IArroganceGainContext
    {
        public readonly bool wasArroganceModeActiveOnExit;
        public readonly float normalizedExitTime;

        public CloseDodgeGainContext(bool wasArroganceModeActiveOnExit, float normalizedExitTime)
        {
            this.wasArroganceModeActiveOnExit = wasArroganceModeActiveOnExit;
            this.normalizedExitTime = normalizedExitTime;
        }
    }
}
