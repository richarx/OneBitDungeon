using System;
using UnityEngine;

namespace Player.Scripts
{

    public interface IAttackStrategy
    {
        void Initialize(PlayerStateMachine player);
        Vector3 ComputeDashTarget(PlayerStateMachine player, AttackPayload attackPayload);
        AttackPayload SelectAttackPayload(int attackCount, TagContext tagContext);
        void OnAttackStart(PlayerStateMachine player, AttackPayload attackPayload);
        void OnTagAttack();
        bool CanAttack(PlayerStateMachine player);
    }
}
