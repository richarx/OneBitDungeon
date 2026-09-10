namespace Tutorials
{
    public static class TutorialTextFormatter
    {
        public const string InputToken = "{input}";
        public const string CurrentToken = "{current}";
        public const string TargetToken = "{target}";

        public static string Format(string template, string input, int current, int target)
        {
            return (template ?? string.Empty)
                .Replace(InputToken, input ?? string.Empty)
                .Replace(CurrentToken, current.ToString())
                .Replace(TargetToken, target.ToString());
        }
    }
}
