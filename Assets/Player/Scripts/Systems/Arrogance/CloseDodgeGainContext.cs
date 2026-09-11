

using UnityEngine;

namespace Player.Scripts
{
    public class CloseDodgeGainContext : IArroganceGainContext
    {
        public readonly bool wasArroganceModeActiveOnExit;
        public readonly float normalizedExitTime;
        public readonly Vector3 outPosition;

        public CloseDodgeGainContext(bool wasArroganceModeActiveOnExit, float normalizedExitTime, Vector3 outPosition)
        {
            this.wasArroganceModeActiveOnExit = wasArroganceModeActiveOnExit;
            this.normalizedExitTime = normalizedExitTime;
            this.outPosition = outPosition;
        }
    }
}
