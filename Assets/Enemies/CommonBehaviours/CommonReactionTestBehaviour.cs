using System;
using Enemies.Scripts;
using Enemies.Scripts.Behaviours;
using PrimeTween;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

/// <summary>
/// Compatibility test behaviour retained because existing scene data references this type.
/// It now exercises only context-dependent weighted selection.
/// </summary>
[Serializable]
public sealed class CommonReactionTestBehaviour : IEnemyBehaviour, IContextualEnemyBehaviour
{
    [TitleGroup("Context weight")]
    [OdinSerialize]
    [MinValue(0.0f)]
    [LabelText("Default weight")]
    private float _defaultWeight = 100.0f;

    [TitleGroup("Context weight")]
    [OdinSerialize]
    [MinValue(0.0f)]
    [LabelText("Close distance")]
    [SuffixLabel("meters")]
    private float _closeDistance = 3.0f;

    [TitleGroup("Context weight")]
    [OdinSerialize]
    [MinValue(0.0f)]
    [LabelText("Weight while close")]
    private float _closeWeight = 100.0f;

    [TitleGroup("Context weight")]
    [OdinSerialize]
    [LabelText("Eligible")]
    private bool _isEligible = true;

    [TitleGroup("Lifetime")]
    [OdinSerialize]
    [MinValue(0.0f)]
    [LabelText("Auto-complete after")]
    [SuffixLabel("seconds")]
    private float _autoCompleteAfter = 1.0f;

    [NonSerialized] private EnemyController _enemy;
    [NonSerialized] private BehaviourExecution _execution;
    [NonSerialized] private Sequence _completionSequence;

    public void StartBehaviour(EnemyController enemy, BehaviourExecution execution)
    {
        ResetRuntimeState();
        _enemy = enemy;
        _execution = execution;

        if (_autoCompleteAfter <= 0.0f)
        {
            execution.Complete();
            return;
        }

        _completionSequence = Sequence.Create()
            .ChainDelay(_autoCompleteAfter)
            .ChainCallback(() =>
            {
                if (_enemy != null && _enemy.IsExecutionActive(_execution))
                    _execution.Complete();
            });
    }

    public void UpdateBehaviour(EnemyController enemy) { }
    public void FixedUpdateBehaviour(EnemyController enemy) { }
    public void StopBehaviour(EnemyController enemy) => ResetRuntimeState();
    public void CancelBehaviour(EnemyController enemy) => ResetRuntimeState();
    public void SetSubBehaviourState(bool state) { }

    public bool CanExecute(EnemyContext context) => _isEligible;

    public float GetWeight(EnemyContext context)
    {
        return context != null && context.HasTarget && context.DistanceToTarget <= _closeDistance
            ? _closeWeight
            : _defaultWeight;
    }

    private void ResetRuntimeState()
    {
        if (_completionSequence.isAlive)
            _completionSequence.Stop();

        _completionSequence = default;
        _enemy = null;
        _execution = null;
    }
}
