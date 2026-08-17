using System;

namespace Player.Scripts
{
    public static class ArroganceGainEvents
    {
        public static event Action<ArroganceGainRequest> OnGainRequested;
        public static event Action<ArroganceGainResult> OnGainProcessed;

        public static void RequestGain(ArroganceGainRequest request)
        {
            OnGainRequested?.Invoke(request);
        }

        public static void PublishProcessedGain(ArroganceGainResult result)
        {
            OnGainProcessed?.Invoke(result);
        }
    }
}
