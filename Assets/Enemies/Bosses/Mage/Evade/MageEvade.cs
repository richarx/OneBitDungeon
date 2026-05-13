using Enemies.Scripts.Behaviours;
using Player.Scripts;
using PrimeTween;
using UnityEngine;

public class MageEvade : MonoBehaviour, IEnemyBehaviour
{
    [SerializeField] private float radius;

    [Space]
    [SerializeField] private MageEvadeSpell mageEvadeSpellPrefab;
    [SerializeField] private MageData mageData;

    private Sequence attackSequence;
    private MageEvadeSpell spell;

    public void StartBehaviour(EnemyController enemy)
    {
        Debug.Log("Mage EVADE");

        bool isSecondPhase = enemy.currentPhase > 0;

        Vector3 currentPosition = enemy.transform.position;
        Vector3 playerPosition = PlayerStateMachine.instance.position;
        Vector3 targetPosition = playerPosition + (currentPosition - playerPosition).normalized * 0.5f;

        Vector3 evadePosition = targetPosition.magnitude <= 0.01f ? Vector3.forward * 7.0f : (targetPosition * -1.0f).normalized * 7.0f;

        attackSequence = Sequence.Create()
            .ChainCallback(() => SpawnDamageZone(targetPosition))
            .Chain(MoveMageToPosition(enemy, targetPosition))
            .Chain(TeleportMageToPosition(enemy, evadePosition))
            .ChainDelay(isSecondPhase ? mageData.evadeRecoveryDuration_p2 : mageData.evadeRecoveryDuration)
            .ChainCallback(() => enemy.SelectNewBehaviour());
    }

    private Sequence MoveMageToPosition(EnemyController enemy, Vector3 position)
    {
        bool isSecondPhase = enemy.currentPhase > 0;

        return Sequence.Create()
            .ChainCallback(() =>
            {
                if (isSecondPhase)
                    enemy.afterImage.Trigger(mageData.evadeSpawnDuration);
                MageSFX.instance.PlayMageMove();
            })
            .Group(Tween.Position(enemy.transform, position, mageData.evadeSpawnDuration, Ease.InOutCubic));
    }

    private Sequence TeleportMageToPosition(EnemyController enemy, Vector3 position)
    {
        return Sequence.Create()
            .ChainCallback(() => enemy.animator.Play("Blast"))
            .Chain(Tween.ScaleX(enemy.transform, 0.0f, 0.3f, Ease.InBack))
            .ChainCallback(() => enemy.transform.position = position)
            .Chain(Tween.ScaleX(enemy.transform, 1.0f, 0.3f, Ease.OutBack));
    }

    private void SpawnDamageZone(Vector3 position)
    {
        spell = Instantiate(mageEvadeSpellPrefab, position, Quaternion.Euler(new Vector3(90.0f, 0.0f, 0.0f)));
        spell.Setup(radius, mageData.evadeSpawnDuration, mageData.evadeFillDuration, null);
    }

    public void UpdateBehaviour(EnemyController enemy)
    {
    }

    public void FixedUpdateBehaviour(EnemyController enemy)
    {
    }

    public void StopBehaviour(EnemyController enemy)
    {
        if (attackSequence.isAlive)
            attackSequence.Stop();
    }

    public void CancelBehaviour(EnemyController enemy)
    {
        if (attackSequence.isAlive)
            attackSequence.Stop();

        if (spell != null)
            spell.Cancel();
    }

    public bool isSubBehaviour;
    public void SetSubBehaviourState(bool state)
    {
        isSubBehaviour = state;
    }
}
