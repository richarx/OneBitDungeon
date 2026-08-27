using System;
using Enemies.Scripts.Behaviours;
using PrimeTween;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

[Serializable]
public sealed class BiscottoFlexBehaviour : IEnemyBehaviour, IConditionalEnemyBehaviour
{
    [OdinSerialize]
    [Required]
    [LabelText("Data")]
    private BiscottoFlexData data;

    [NonSerialized] private Sequence flexSequence;

    public void StartBehaviour(EnemyController enemy, BehaviourExecution execution)
    {
        ResetRuntimeState();

        if (data == null)
        {
            UnityEngine.Debug.LogError("[BiscottoFlexBehaviour] Un data de flex est requis.", enemy);
            execution.Complete();
            return;
        }

        BiscottoArrogance arrogance = enemy.GetComponent<BiscottoArrogance>();
        if (arrogance == null)
        {
            UnityEngine.Debug.LogError("[BiscottoFlexBehaviour] BiscottoArrogance est requis sur le boss.", enemy);
            execution.Complete();
            return;
        }

        PlayAnimation(enemy, SelectAnimationFromArrogance(arrogance.CurrentArroganceLevel, data));
        flexSequence = Sequence.Create()
            .ChainDelay(data.FlexDuration)
            .ChainCallback(arrogance.AddArroganceLevel)
            .ChainDelay(data.RecoveryDuration)
            .ChainCallback(() => PlayAnimation(enemy, "Idle"))
            .ChainCallback(() => execution.Complete());
    }

    private string SelectAnimationFromArrogance(int currentArroganceLevel, BiscottoFlexData data)
    {
        if (currentArroganceLevel == 0)
            return data.FlexAnimation_Arrogance_1;
        if (currentArroganceLevel == 1)
            return data.FlexAnimation_Arrogance_2;
        return data.FlexAnimation_Arrogance_3;
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
