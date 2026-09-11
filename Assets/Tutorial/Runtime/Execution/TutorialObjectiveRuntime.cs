using System;

namespace Tutorials
{
    internal sealed class TutorialObjectiveRuntime : IDisposable
    {
        private readonly TutorialObjectiveData _data;
        private readonly ITutorialPresenter _presenter;


        public TutorialObjectiveRuntime(TutorialObjectiveData data, ITutorialPresenter presenter)
        {
            _data = data ?? throw new ArgumentNullException(nameof(data));
            _presenter = presenter ?? throw new ArgumentNullException(nameof(presenter));

            TutorialSignalBridge.Instance.SubToBridge(HandleSignal);
        }

        public bool IsCompleted { get; private set; }

        public TutorialObjectiveData Data => _data;

        private int Current { get; set; }

        public void Dispose()
        {
            TutorialSignalBridge bridge = TutorialSignalBridge.Instance;
            if (bridge != null)
                bridge.UnsubFromBridge(HandleSignal);
        }

        private void HandleSignal(TutorialSignal signal)
        {
            if (IsCompleted || signal.Id != _data.SignalId)
            {
                return;
            }

            Current = Math.Min(Current + 1, _data.Target);
            _presenter.SetObjectiveProgress(_data.Id, Current, _data.Target);

            if (Current < _data.Target)
                return;

            IsCompleted = true;
            _presenter.CompleteObjective(_data.Id);
            Dispose();
        }
    }
}
