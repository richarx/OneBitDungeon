using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Enemies.BehaviourSequencer
{
    [Serializable]
    public sealed class PlaySFXBehaviourAction : EnemyBehaviourAction
    {
        public enum MageSFXType
        {
            Move,
            RockMove,
            RockBreak,
            RockThrow,
        }

        [FoldoutGroup("SFX")]
        [SerializeField]
        private MageSFXType sfxType;

        public override UniTask ExecuteAsync(BehaviourContext context, CancellationToken cancellationToken)
        {
            switch (sfxType)
            {
                case MageSFXType.Move:
                    MageSFX.instance.PlayMageMove();
                    break;
                case MageSFXType.RockMove:
                    MageSFX.instance.PlayRockMove();
                    break;
                case MageSFXType.RockBreak:
                    MageSFX.instance.PlayRockBreak();
                    break;
                case MageSFXType.RockThrow:
                    MageSFX.instance.PlayRockThrow();
                    break;
            }

            return UniTask.CompletedTask;
        }
    }
}
