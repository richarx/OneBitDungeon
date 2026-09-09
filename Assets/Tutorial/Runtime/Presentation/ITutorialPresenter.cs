using System.Collections.Generic;

namespace Tutorials
{
    public interface ITutorialPresenter
    {
        void ShowObjectives(IReadOnlyList<TutorialObjectiveData> objectives);

        void SetObjectiveProgress(string objectiveId, int current, int target);

        void CompleteObjective(string objectiveId);

        void HideObjectives();
    }
}
