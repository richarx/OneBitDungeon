
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace Player.Scripts
{
    [CreateAssetMenu(fileName = "CharacterDefinition", menuName = "ScriptableObjects/CharacterDefinition")]
    public class CharacterDefinition : SerializedScriptableObject  // ← était ScriptableObject
    {
        [Header("Data")]
        public PlayerData playerData;
        public int maxHealth;

        [Header("Visuals")]
        public RuntimeAnimatorController animatorController;

        [Header("Attack")]
        [OdinSerialize] public IAttackStrategy attackStrategy;  // ← était [SerializeReference]
    }
}
