using System;
using Enemies.Scripts.Behaviours;
using Player.Scripts;
using PrimeTween;
using UnityEngine;

public class MageEvade : MonoBehaviour, IEnemyBehaviour
{
    [SerializeField] private MageEvadeSpell mageEvadeSpellPrefab;

    private float radius = 3.0f;
    private float spawnDuration = 0.5f;
    private float fillDuration = 1.0f;

    private Sequence attackSequence;

    public void StartBehaviour(EnemyController enemy)
    {
        Debug.Log("Mage EVADE");

        Vector3 currentPosition = enemy.transform.position;
        Vector3 playerPosition = PlayerStateMachine.instance.position;
        Vector3 targetPosition = playerPosition + (currentPosition - playerPosition).normalized * 0.5f;

        Vector3 evadePosition = targetPosition.magnitude <= 0.01f ? Vector3.forward * 7.0f : (targetPosition * -1.0f).normalized * 7.0f;

        attackSequence = Sequence.Create()
            .ChainCallback(() => SpawnDamageZone(targetPosition))
            .Chain(MoveMageToPosition(enemy, targetPosition))
            .Chain(TeleportMageToPosition(enemy, targetPosition))
            .ChainCallback(() => enemy.SelectNewBehaviour());
    }

    private Sequence MoveMageToPosition(EnemyController enemy, Vector3 position)
    {
        bool isSecondPhase = enemy.currentPhase > 0;

        return Sequence.Create()
            .ChainCallback(() =>
            {
                if (isSecondPhase)
                    enemy.afterImage.Trigger(spawnDuration);
                MageSFX.instance.PlayMageMove();
            })
            .Group(Tween.Position(enemy.transform, position, spawnDuration, Ease.InOutCubic));
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
        MageEvadeSpell spell = Instantiate(mageEvadeSpellPrefab, position, Quaternion.Euler(new Vector3(90.0f, 0.0f, 0.0f)));
        spell.Setup(radius, spawnDuration, fillDuration, null);
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
    }

    public bool isSubBehaviour;
    public void SetSubBehaviourState(bool state)
    {
        isSubBehaviour = state;
    }
}
