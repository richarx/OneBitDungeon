using UnityEngine;

namespace Player.Scripts
{
    public class ArroganceGainRequest
    {
        public readonly float baseAmount;
        public readonly ArroganceGainReason reason;

        public readonly Object source;
        public readonly IArroganceGainContext context;

        public ArroganceGainRequest(float baseAmount, ArroganceGainReason reason, Object source = null, IArroganceGainContext context = null)
        {
            this.baseAmount = baseAmount;
            this.reason = reason;
            this.source = source;
            this.context = context;
        }
    }
}
