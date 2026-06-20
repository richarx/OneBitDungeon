using System;

namespace TataSequencing
{
    [Serializable]
    public sealed class LoopAnchorAction : SequencerAction
    {
        public override bool IsControlFlow => true;
    }
}
