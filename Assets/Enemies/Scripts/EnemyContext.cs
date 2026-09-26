using Enemies.Scripts.Behaviours;
using Player.Scripts;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Enemies.Scripts
{
    public enum EnemyExchangeResult
    {
        None,
        PlayerBlocked,
        PlayerDeflected,
        EnemyWasHit,
        PlayerCloseDodged,
        PlayerRolledNearby
    }

    /// <summary>
    /// Runtime-only information owned by one EnemyController. Behaviours may read it,
    /// but only the controller records exchange results and refreshes spatial data.
    /// </summary>
    public sealed class EnemyContext
    {
        [ShowInInspector, ReadOnly] public Transform Target { get; private set; }
        [ShowInInspector, ReadOnly] public PlayerStateMachine Player { get; private set; }
        [ShowInInspector, ReadOnly] public Vector3 TargetPosition { get; private set; }
        [ShowInInspector, ReadOnly] public Vector3 DirectionToTarget { get; private set; }
        [ShowInInspector, ReadOnly] public float DistanceToTarget { get; private set; }
        [ShowInInspector, ReadOnly] public bool HasTarget { get; private set; }

        [ShowInInspector, ReadOnly] public int CurrentPhase { get; private set; }
        [ShowInInspector, ReadOnly] public IEnemyBehaviour CurrentBehaviour { get; private set; }

        [ShowInInspector, ReadOnly] public EnemyExchangeResult LastExchangeResult { get; private set; }
        [ShowInInspector, ReadOnly] public float LastExchangeUnscaledTime { get; private set; } = float.NegativeInfinity;
        [ShowInInspector, ReadOnly] public int ExchangeCount { get; private set; }
        [ShowInInspector, ReadOnly] public int ConsecutiveExchangeCount { get; private set; }

        [ShowInInspector, ReadOnly] public bool IsPlayerRolling { get; private set; }
        [ShowInInspector, ReadOnly] public float LastRollStartUnscaledTime { get; private set; } = float.NegativeInfinity;
        [ShowInInspector, ReadOnly] public float LastRollEndUnscaledTime { get; private set; } = float.NegativeInfinity;
        [ShowInInspector, ReadOnly] public float DistanceAtRollStart { get; private set; }
        [ShowInInspector, ReadOnly] public bool IsPlayerNearby { get; private set; }
        [ShowInInspector, ReadOnly] public float NearbySinceUnscaledTime { get; private set; } = float.NegativeInfinity;
        [ShowInInspector, ReadOnly] public float NearbyDuration { get; private set; }

        internal void Reset(int currentPhase)
        {
            CurrentPhase = currentPhase;
            CurrentBehaviour = null;
            LastExchangeResult = EnemyExchangeResult.None;
            LastExchangeUnscaledTime = float.NegativeInfinity;
            ExchangeCount = 0;
            ConsecutiveExchangeCount = 0;
            IsPlayerRolling = false;
            LastRollStartUnscaledTime = float.NegativeInfinity;
            LastRollEndUnscaledTime = float.NegativeInfinity;
            DistanceAtRollStart = 0.0f;
            IsPlayerNearby = false;
            NearbySinceUnscaledTime = float.NegativeInfinity;
            NearbyDuration = 0.0f;
            ClearSpatialData();
        }

        internal void SetPlayer(PlayerStateMachine player)
        {
            Player = player;
        }

        internal void SetTarget(Transform target)
        {
            Target = target;
            if (Target == null)
                ClearSpatialData();
        }

        internal void RefreshForDecision(
            Transform enemyTransform,
            int currentPhase,
            IEnemyBehaviour currentBehaviour,
            float exchangeMemoryDuration,
            float nearbyRadius)
        {
            CurrentPhase = currentPhase;
            CurrentBehaviour = currentBehaviour;
            ExpireExchangeMemory(exchangeMemoryDuration);

            if (enemyTransform == null || Target == null)
            {
                ClearSpatialData();
                ClearNearbyData();
                return;
            }

            TargetPosition = Target.position;
            Vector3 offset = TargetPosition - enemyTransform.position;
            offset.y = 0.0f;
            DistanceToTarget = offset.magnitude;
            DirectionToTarget = DistanceToTarget > Mathf.Epsilon ? offset / DistanceToTarget : Vector3.zero;
            HasTarget = true;
            RefreshNearbyData(nearbyRadius);
        }

        internal void SetCurrentBehaviour(IEnemyBehaviour behaviour)
        {
            CurrentBehaviour = behaviour;
        }

        internal void SetCurrentPhase(int phase)
        {
            CurrentPhase = phase;
        }

        internal void RecordExchange(EnemyExchangeResult result)
        {
            if (result == EnemyExchangeResult.None)
                return;

            ConsecutiveExchangeCount = LastExchangeResult == result
                ? ConsecutiveExchangeCount + 1
                : 1;
            LastExchangeResult = result;
            LastExchangeUnscaledTime = Time.unscaledTime;
            ExchangeCount++;
        }

        internal void RecordPlayerRollStarted()
        {
            IsPlayerRolling = true;
            LastRollStartUnscaledTime = Time.unscaledTime;
            DistanceAtRollStart = DistanceToTarget;
        }

        internal void RecordPlayerRollEnded()
        {
            IsPlayerRolling = false;
            LastRollEndUnscaledTime = Time.unscaledTime;
        }

        internal void ConsumeExchangeMemory()
        {
            LastExchangeResult = EnemyExchangeResult.None;
            ConsecutiveExchangeCount = 0;
        }

        public float GetExchangeAge()
        {
            return LastExchangeResult == EnemyExchangeResult.None
                ? float.PositiveInfinity
                : Mathf.Max(0.0f, Time.unscaledTime - LastExchangeUnscaledTime);
        }

        private void ExpireExchangeMemory(float duration)
        {
            if (LastExchangeResult == EnemyExchangeResult.None
                || Time.unscaledTime - LastExchangeUnscaledTime <= Mathf.Max(0.0f, duration))
                return;

            LastExchangeResult = EnemyExchangeResult.None;
            ConsecutiveExchangeCount = 0;
        }

        private void ClearSpatialData()
        {
            TargetPosition = Vector3.zero;
            DirectionToTarget = Vector3.zero;
            DistanceToTarget = 0.0f;
            HasTarget = false;
        }

        private void RefreshNearbyData(float nearbyRadius)
        {
            if (DistanceToTarget > Mathf.Max(0.0f, nearbyRadius))
            {
                ClearNearbyData();
                return;
            }

            if (!IsPlayerNearby)
            {
                IsPlayerNearby = true;
                NearbySinceUnscaledTime = Time.unscaledTime;
            }

            NearbyDuration = Mathf.Max(0.0f, Time.unscaledTime - NearbySinceUnscaledTime);
        }

        private void ClearNearbyData()
        {
            IsPlayerNearby = false;
            NearbySinceUnscaledTime = float.NegativeInfinity;
            NearbyDuration = 0.0f;
        }
    }
}
