namespace Tutorials
{
    public readonly struct TutorialSignal
    {
        public TutorialSignal(TutorialSignalId id)
        {
            Id = id;
        }

        public TutorialSignalId Id { get; }

    }
}
