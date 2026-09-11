using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tutorials
{
    [Serializable]
    public sealed class AttackTutorial : ITutorial
    {
        [SerializeField, Required]
        private AttackTutorialData _data;

        public TutorialData Data => _data;

        public string ListLabel => _data != null
            ? _data.EditorLabel
            : nameof(AttackTutorial);

        public async UniTask ExecuteAsync(TutorialContext context, CancellationToken cancellationToken)
        {
            if (_data == null)
                throw new InvalidOperationException("AttackTutorial requires an AttackTutorialData asset.");

            if (context?.Runner == null)
                throw new InvalidOperationException("AttackTutorial requires a TutorialRunner in its context.");

            await context.Runner.RunAsync(_data, cancellationToken);

            
        }
    }
}
