using System;
using Enemies.Scripts;
using Enemies.Scripts.Behaviours;
using PrimeTween;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

/// <summary>
/// Context-weight test behaviour kept under its original type name for scene compatibility.
/// Its editable configuration lives in CommonReactionTestData assets.
/// </summary>
[Serializable]
public sealed class CommonReactionTestBehaviour : IEnemyBehaviour, IContextualEnemyBehaviour
{
    [OdinSerialize]
    [Required]
    [LabelText("Data")]
    [InfoBox("Assign a Common Reaction Test Data asset. This behaviour is not eligible without one.", InfoMessageType.Error, nameof(HasMissingData))]
    private CommonReactionTestData _data;

    [NonSerialized] private EnemyController _enemy;
    [NonSerialized] private BehaviourExecution _execution;
    [NonSerialized] private Sequence _completionSequence;

    private bool HasMissingData => _data == null;

    public void StartBehaviour(EnemyController enemy, BehaviourExecution execution)
    {
        ResetRuntimeState();
        _enemy = enemy;
        _execution = execution;

        if (_data == null || _data.AutoCompleteAfter <= 0.0f)
        {
            execution.Complete();
            return;
        }

        _completionSequence = Sequence.Create()
            .ChainDelay(_data.AutoCompleteAfter)
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

    public bool CanExecute(EnemyContext context) => _data != null && _data.IsEligible;

    public float GetWeight(EnemyContext context)
    {
        if (_data == null)
            return 0.0f;

        return context != null && context.HasTarget && context.DistanceToTarget <= _data.CloseDistance
            ? _data.CloseWeight
            : _data.DefaultWeight;
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
