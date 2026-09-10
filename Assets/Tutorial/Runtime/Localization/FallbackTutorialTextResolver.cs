namespace Tutorials
{
    public sealed class FallbackTutorialTextResolver : ITutorialTextResolver
    {
        public string Resolve(TutorialText text)
        {
            return text != null ? text.FallbackText : string.Empty;
        }
    }
}
