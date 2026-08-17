using System;

namespace Player.Scripts
{
    public static class ArroganceGainEvents
    {
        public static event Action<ArroganceGainRequest> OnGainRequested;

        public static void RequestGain(ArroganceGainRequest request)
        {
            OnGainRequested?.Invoke(request);
        }
    }
}
