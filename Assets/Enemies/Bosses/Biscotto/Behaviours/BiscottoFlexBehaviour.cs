using System;
using Enemies.Scripts.Behaviours;
using PrimeTween;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

[Serializable]
public sealed class BiscottoFlexBehaviour : IEnemyBehaviour, IConditionalEnemyBehaviour
{
    [OdinSerialize]
    [MinValue(0.0f)]
    [LabelText("Durée du flex")]
    private float flexDuration = 1.2f;

    [OdinSerialize]
    [MinValue(0.0f)]
    [LabelText("Arrogance gagnée")]
    private float arroganceGain = 34.0f;

    [OdinSerialize]
    [MinValue(0.0f)]
    [LabelText("Récupération")]
    private float recoveryDuration = 0.35f;

    [OdinSerialize]
    [LabelText("Animation")]
    private string flexAnimation;

    [NonSerialized] private Sequence flexSequence;

    public void StartBehaviour(EnemyController enemy, BehaviourExecution execution)
    {
        ResetRuntimeState();

        BiscottoArrogance arrogance = enemy.GetComponent<BiscottoArrogance>();
        if (arrogance == null)
        {
            UnityEngine.Debug.LogError("[BiscottoFlexBehaviour] BiscottoArrogance est requis sur le boss.", enemy);
            execution.Complete();
            return;
        }

        PlayAnimation(enemy, flexAnimation);
        flexSequence = Sequence.Create()
            .ChainDelay(flexDuration)
            .ChainCallback(() => arrogance.AddArrogance(arroganceGain))
            .ChainDelay(recoveryDuration)
            .ChainCallback(() => execution.Complete());
    }

    public void UpdateBehaviour(EnemyController enemy)
    {
    }

    public void FixedUpdateBehaviour(EnemyController enemy)
    {
    }

    public void StopBehaviour(EnemyController enemy)
    {
        ResetRuntimeState();
    }

    public void CancelBehaviour(EnemyController enemy)
    {
        ResetRuntimeState();
    }

    public void SetSubBehaviourState(bool state)
    {
    }

    public bool CanExecute(EnemyController enemy)
    {
        BiscottoArrogance arrogance = enemy != null ? enemy.GetComponent<BiscottoArrogance>() : null;
        return arrogance != null && !arrogance.IsFull;
    }

    private void ResetRuntimeState()
    {
        if (flexSequence.isAlive)
            flexSequence.Stop();

        flexSequence = default;
    }

    private static void PlayAnimation(EnemyController enemy, string animationName)
    {
        if (enemy.animator != null && !string.IsNullOrWhiteSpace(animationName))
            enemy.animator.Play(animationName);
    }
}
