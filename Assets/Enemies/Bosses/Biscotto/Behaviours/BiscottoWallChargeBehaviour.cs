using System;
using Enemies.Scripts.Behaviours;
using Player.Scripts;
using PrimeTween;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Tools_and_Scripts;
using UnityEngine;

[Serializable]
public sealed class BiscottoWallChargeBehaviour : IEnemyBehaviour
{
    private const float _MinimumDirectionSqrMagnitude = 0.0001f;

    [OdinSerialize]
    [Required]
    [LabelText("Data")]
    private BiscottoWallChargeData _data;

    private Sequence _attackSequence;
    private Sequence _chargeSequence;
    private RectangleDamageZone _currentDamageZone;
    private Transform _currentDamageZoneRoot;
    private BehaviourExecution _currentExecution;
    private float _aimEndTimestamp;
    private float _zoneResizeStartTimestamp;

    public void StartBehaviour(EnemyController enemy, BehaviourExecution execution)
    {
        ResetRuntimeState();

        if (_data == null)
        {
            Debug.LogError("[BiscottoWallChargeBehaviour] Un data de charge est requis.", enemy);
            execution.Complete();
            return;
        }

        if (_data.RectangularDamageZonePrefab == null)
        {
            Debug.LogError("[BiscottoWallChargeBehaviour] Un prefab de zone rectangulaire est requis.", enemy);
            execution.Complete();
            return;
        }

        if (PlayerStateMachine.instance == null)
        {
            Debug.LogError("[BiscottoWallChargeBehaviour] Aucun joueur n'est disponible.", enemy);
            execution.Complete();
            return;
        }

        _currentExecution = execution;
        Vector3 backPosition = ToWorldPosition(_data.BackPosition, enemy.transform.position.y);

        _attackSequence = Sequence.Create()
            .ChainCallback(() => BeginMoveToBack(enemy))
            .Chain(BiscottoMovementUtility.CreateLinearMove(
                enemy,
                backPosition,
                _data.MoveToBackDuration,
                Ease.InOutSine))
            .ChainCallback(() =>
            {
                PlayAnimation(enemy, _data.AnticipationAnimation);
                SpawnAttackZone(enemy);
            })
            .ChainDelay(_data.SpawnDuration + _data.FillDuration)
            .ChainCallback(() => BeginCharge(enemy));
    }

    public void UpdateBehaviour(EnemyController enemy)
    {
        if (_currentDamageZoneRoot == null || Time.time >= _aimEndTimestamp)
            return;

        RotateZoneTowardPlayer(enemy);

        if (Time.time >= _zoneResizeStartTimestamp)
        {
            float chargeDistance = ComputeDistanceToWall(enemy.transform.position, _currentDamageZoneRoot.right);
            _currentDamageZone.UpdateDimensions(_data.DamageZoneWidth, chargeDistance);
        }
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

    private void BeginMoveToBack(EnemyController enemy)
    {
        PlayAnimation(enemy, _data.MoveAnimation);

        if (_data.TriggerAfterImageOnMove && enemy.afterImage != null)
            enemy.afterImage.Trigger(_data.MoveToBackDuration);
    }

    private void SpawnAttackZone(EnemyController enemy)
    {
        GameObject zoneObject = UnityEngine.Object.Instantiate(
            _data.RectangularDamageZonePrefab,
            enemy.transform.position,
            Quaternion.identity);
        RectangleDamageZone damageZone = zoneObject.GetComponentInChildren<RectangleDamageZone>();

        if (damageZone == null)
        {
            Debug.LogError(
                $"[{enemy.name}] Le prefab '{_data.RectangularDamageZonePrefab.name}' ne contient pas de RectangleDamageZone.",
                _data.RectangularDamageZonePrefab);
            UnityEngine.Object.Destroy(zoneObject);
            CompleteIfActive(enemy);
            return;
        }

        _currentDamageZone = damageZone;
        _currentDamageZoneRoot = zoneObject.transform;
        _aimEndTimestamp = Time.time + Mathf.Max(
            0.0f,
            _data.SpawnDuration + _data.FillDuration - _data.LockBeforeCharge);
        _zoneResizeStartTimestamp = Time.time + _data.SpawnDuration;

        RotateZoneTowardPlayer(enemy, true);
        float chargeDistance = ComputeDistanceToWall(enemy.transform.position, _currentDamageZoneRoot.right);
        damageZone.SetDimensions(_data.DamageZoneWidth, chargeDistance);
        damageZone.Setup(Vector2.right, _data.SpawnDuration, _data.FillDuration);
    }

    private void RotateZoneTowardPlayer(EnemyController enemy, bool immediate = false)
    {
        if (_currentDamageZoneRoot == null || PlayerStateMachine.instance == null)
            return;

        _currentDamageZoneRoot.position = enemy.transform.position;

        Vector3 direction = PlayerStateMachine.instance.position - enemy.transform.position;
        direction.y = 0.0f;
        if (direction.sqrMagnitude <= _MinimumDirectionSqrMagnitude)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(
            direction.normalized.ToVector2().AddAngleToDirection(90.0f).ToVector3());

        if (immediate)
        {
            _currentDamageZoneRoot.rotation = targetRotation;
            return;
        }

        _currentDamageZoneRoot.rotation = Quaternion.Slerp(
            _currentDamageZoneRoot.rotation,
            targetRotation,
            Time.deltaTime / Mathf.Max(0.001f, _data.RotationDampening));
    }

    private void BeginCharge(EnemyController enemy)
    {
        if (!enemy.IsExecutionActive(_currentExecution))
            return;

        Vector3 direction = GetLockedChargeDirection(enemy);
        float chargeDistance = ComputeDistanceToWall(enemy.transform.position, direction);
        if (_currentDamageZone != null)
            _currentDamageZone.UpdateDimensions(_data.DamageZoneWidth, chargeDistance);

        Vector3 destination = enemy.transform.position + direction * chargeDistance;
        destination.y = enemy.transform.position.y;

        PlayAnimation(enemy, _data.ChargeAnimation);
        if (_data.TriggerAfterImageOnCharge && enemy.afterImage != null)
            enemy.afterImage.Trigger(_data.ChargeDuration);

        _chargeSequence = Sequence.Create()
            .Chain(BiscottoMovementUtility.CreateLinearMove(
                enemy,
                destination,
                _data.ChargeDuration,
                Ease.InQuad))
            .ChainCallback(() =>
            {
                PlayAnimation(enemy, _data.ImpactAnimation);
                SqueezeAndStretch squeezeAndStretch = enemy.GetComponent<SqueezeAndStretch>();
                if (squeezeAndStretch != null)
                    squeezeAndStretch.Trigger();
            })
            .ChainDelay(_data.ImpactDuration)
            .ChainCallback(() => PlayAnimation(enemy, _data.RecoveryAnimation))
            .ChainDelay(_data.RecoveryDuration)
            .ChainCallback(() =>
            {
                PlayAnimation(enemy, "Idle");
                CompleteIfActive(enemy);
            });
    }

    private Vector3 GetLockedChargeDirection(EnemyController enemy)
    {
        if (_currentDamageZoneRoot != null)
        {
            Vector3 zoneDirection = _currentDamageZoneRoot.right;
            zoneDirection.y = 0.0f;
            if (zoneDirection.sqrMagnitude > _MinimumDirectionSqrMagnitude)
                return zoneDirection.normalized;
        }

        Vector3 fallbackDirection = PlayerStateMachine.instance != null
            ? PlayerStateMachine.instance.position - enemy.transform.position
            : Vector3.back;
        fallbackDirection.y = 0.0f;
        return fallbackDirection.sqrMagnitude > _MinimumDirectionSqrMagnitude
            ? fallbackDirection.normalized
            : Vector3.back;
    }

    private float ComputeDistanceToWall(Vector3 origin, Vector3 direction)
    {
        Vector2 halfSize = new Vector2(
            Mathf.Max(0.01f, _data.ArenaHalfSize.x - _data.WallInset),
            Mathf.Max(0.01f, _data.ArenaHalfSize.y - _data.WallInset));
        float minX = _data.ArenaCenter.x - halfSize.x;
        float maxX = _data.ArenaCenter.x + halfSize.x;
        float minZ = _data.ArenaCenter.y - halfSize.y;
        float maxZ = _data.ArenaCenter.y + halfSize.y;
        float xDistance = GetAxisDistance(origin.x, direction.x, minX, maxX);
        float zDistance = GetAxisDistance(origin.z, direction.z, minZ, maxZ);
        float distance = Mathf.Min(xDistance, zDistance);

        return float.IsInfinity(distance) ? 0.0f : Mathf.Max(0.0f, distance);
    }

    private static float GetAxisDistance(float origin, float direction, float minimum, float maximum)
    {
        if (direction > Mathf.Epsilon)
            return Mathf.Max(0.0f, (maximum - origin) / direction);

        if (direction < -Mathf.Epsilon)
            return Mathf.Max(0.0f, (minimum - origin) / direction);

        return float.PositiveInfinity;
    }

    private static Vector3 ToWorldPosition(Vector2 position, float height)
    {
        return new Vector3(position.x, height, position.y);
    }

    private void CompleteIfActive(EnemyController enemy)
    {
        if (enemy.IsExecutionActive(_currentExecution))
            _currentExecution.Complete();
    }

    private void ResetRuntimeState()
    {
        if (_attackSequence.isAlive)
            _attackSequence.Stop();

        if (_chargeSequence.isAlive)
            _chargeSequence.Stop();

        if (_currentDamageZone != null)
            _currentDamageZone.Cancel();

        _attackSequence = default;
        _chargeSequence = default;
        _currentDamageZone = null;
        _currentDamageZoneRoot = null;
        _currentExecution = null;
        _aimEndTimestamp = 0.0f;
        _zoneResizeStartTimestamp = 0.0f;
    }

    private static void PlayAnimation(EnemyController enemy, string animationName)
    {
        if (enemy.animator != null && !string.IsNullOrWhiteSpace(animationName))
            enemy.animator.Play(animationName);
    }
}
