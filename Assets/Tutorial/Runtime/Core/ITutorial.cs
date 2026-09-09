using System.Threading;
using Cysharp.Threading.Tasks;

namespace Tutorials
{
    public interface ITutorial
    {
        TutorialData Data { get; }

        string ListLabel { get; }

        UniTask ExecuteAsync(TutorialContext context, CancellationToken cancellationToken);
    }
}
