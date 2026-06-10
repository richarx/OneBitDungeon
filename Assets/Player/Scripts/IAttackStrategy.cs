using System;
using UnityEngine;

namespace Player.Scripts
{

    public interface IAttackStrategy
    {
        void Initialize(PlayerStateMachine player);
        Vector3 ComputeDashTarget(PlayerStateMachine player);
        string SelectAttackName(int attackCount);
        bool CanAttack(PlayerStateMachine player);
        void OnTagIn(PlayerStateMachine player);
    }
}
