namespace Player.Scripts
{
    public class ArroganceGainResult
    {
        public readonly ArroganceGainRequest request;
        public readonly float totalAmount;

        // Add data from the request that you want to store in the result, such as the reason, source, and context.

        // public readonly float multiplier; // Example of additional data you might want to store

        public ArroganceGainResult(ArroganceGainRequest request, float totalAmount)
        {
            this.request = request;
            this.totalAmount = totalAmount;
        }
    }
}
