using System;
using System.Reflection;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TataSequencing
{
    [Serializable]
    public class SequencerControlAction : SequencerAction
    {
        public enum SequencerMode
        {
            Launch,
            Stop,
            StopAndLaunch
        }

        [FoldoutGroup("Target")]
        [Required]
        [SerializeField]
        private Sequencer targetSequencer;

        [FoldoutGroup("Options")]
        [SerializeField]
        private SequencerMode sequencerMode = SequencerMode.Launch;

        [FoldoutGroup("Options")]
        [SerializeField]
        private bool waitAction = false;

        public override async UniTask ExecuteAsync(SequencerContext context, CancellationToken cancellationToken)
        {
            if (targetSequencer == null)
            {
                Debug.LogWarning($"[{context.Owner.name}] SequencerControlAction has no target sequencer.", context.Owner);
                return;
            }

            switch (sequencerMode)
            {
                case SequencerMode.Launch:
                    if (waitAction)
                    {
                        await targetSequencer.PlayAsync().AttachExternalCancellation(cancellationToken);
                    }
                    else
                    {
                        targetSequencer.Play();
                    }
                    break;

                case SequencerMode.Stop:
                    targetSequencer.Stop();
                    if (waitAction)
                    {
                        await UniTask.WaitUntil(
                            predicate: () => targetSequencer == null || targetSequencer.IsStopped,
                            cancellationToken: cancellationToken);
                    }
                    break;

                case SequencerMode.StopAndLaunch:
                    if (waitAction)
                    {
                        await targetSequencer.StopAndPlayAsync().AttachExternalCancellation(cancellationToken);
                    }
                    else
                    {
                        targetSequencer.StopAndPlay();
                    }
                    break;
            }
        }

        public override void Cancel(SequencerContext context)
        {
        }


    }
}
