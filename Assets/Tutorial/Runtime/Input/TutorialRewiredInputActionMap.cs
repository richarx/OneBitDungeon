using Tools_and_Scripts.RewiredInput;

namespace Tutorials
{
    /// <summary>Maps tutorial-facing actions to their Rewired action names.</summary>
    public static class TutorialRewiredInputActionMap
    {
        public static bool TryGetActionNames(
            TutorialInputAction action,
            out string primaryActionName,
            out string secondaryActionName)
        {
            secondaryActionName = string.Empty;

            switch (action)
            {
                case TutorialInputAction.Move:
                    primaryActionName = RewiredActionNames.MoveHorizontal;
                    secondaryActionName = RewiredActionNames.MoveVertical;
                    return true;

                case TutorialInputAction.Attack:
                    primaryActionName = RewiredActionNames.Attack;
                    return true;

                case TutorialInputAction.Parry:
                    primaryActionName = RewiredActionNames.Parry;
                    return true;

                case TutorialInputAction.Roll:
                    primaryActionName = RewiredActionNames.Roll;
                    return true;

                case TutorialInputAction.Jump:
                    primaryActionName = RewiredActionNames.Jump;
                    return true;

                case TutorialInputAction.CriticalAttack:
                    primaryActionName = RewiredActionNames.Critical;
                    return true;

                case TutorialInputAction.ArroganceMode:
                    primaryActionName = RewiredActionNames.ArroganceMode;
                    return true;

                default:
                    primaryActionName = string.Empty;
                    return false;
            }
        }
    }
}
