using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TataSequencing
{
    [Serializable]
    public sealed class LoopAction : SequencerAction
    {
        [FoldoutGroup("Loop")]
        [SerializeField]
        private bool infinite;

        [FoldoutGroup("Loop")]
        [ShowIf(nameof(ShowRepeatCount))]
        [MinValue(0)]
        [SerializeField]
        [Tooltip("Number of extra repeats after the first pass. 0 = no repeat, 1 = play the block twice total, etc.")]
        private int repeatCount;

        [NonSerialized]
        private int performedRepeats;

        public override bool IsControlFlow => true;

        private bool ShowRepeatCount => !infinite;

        public bool ShouldJump()
        {
            if (infinite)
            {
                return true;
            }

            if (performedRepeats >= repeatCount)
            {
                return false;
            }

            performedRepeats++;
            return true;
        }

        public override void ResetRuntimeState()
        {
            performedRepeats = 0;
        }
    }
}
