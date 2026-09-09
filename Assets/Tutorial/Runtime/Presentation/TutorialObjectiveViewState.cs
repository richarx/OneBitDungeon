using System;
using UnityEngine;

namespace Tutorials
{
    public sealed class TutorialObjectiveViewState
    {
        public TutorialObjectiveViewState(TutorialObjectiveData data)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
            Target = data.Target;
        }

        public TutorialObjectiveData Data { get; }

        public int Current { get; private set; }

        public int Target { get; private set; }

        public bool IsCompleted { get; private set; }

        public void SetProgress(int current, int target)
        {
            Target = Mathf.Max(1, target);
            Current = Mathf.Clamp(current, 0, Target);
        }

        public void Complete()
        {
            IsCompleted = true;

            if (Data.ShowProgress)
                Current = Target;
        }
    }
}
