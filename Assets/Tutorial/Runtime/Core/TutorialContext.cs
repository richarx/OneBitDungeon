namespace Tutorials
{
    public sealed class TutorialContext
    {
        public TutorialContext(TutorialRunner runner)
        {
            Runner = runner;
        }

        public TutorialRunner Runner { get; }
    }
}
