namespace Tutorials
{

    // For latter to add localization support

    public sealed class TutorialTextResolver
    {
        public string Resolve(TutorialText text)
        {
            return text != null ? text.FallbackText : string.Empty;
        }
    }
}
