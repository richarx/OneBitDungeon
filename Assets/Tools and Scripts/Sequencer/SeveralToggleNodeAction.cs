using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TataSequencing
{
    [Serializable]
    public class SeveralToggleGameObjectAction : SequencerAction
    {
        [ListDrawerSettings(ShowFoldout = true)]
        [SerializeField]
        private List<GameObject> targetObjects = new();

        [SerializeField]
        private bool active = true;

        public override UniTask ExecuteAsync(SequencerContext context, CancellationToken cancellationToken)
        {
            if (targetObjects == null || targetObjects.Count == 0)
            {
                Debug.LogWarning($"{nameof(SeveralToggleGameObjectAction)}: No target objects assigned.");
                return UniTask.CompletedTask;
            }

            foreach (var targetObject in targetObjects)
            {
                if (targetObject == null)
                {
                    Debug.LogWarning($"{nameof(SeveralToggleGameObjectAction)}: One of the target objects is null.");
                    continue;
                }

                _ = targetObject.transform.DOKill(complete: false);
                targetObject.SetActive(active);
            }

            return UniTask.CompletedTask;
        }
    }
}
