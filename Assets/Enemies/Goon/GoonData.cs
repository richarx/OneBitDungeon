using UnityEngine;

namespace Enemies.Goon
{
    [CreateAssetMenu(fileName = "GoonData", menuName = "ScriptableObjects/GoonData")]
    public class GoonData : ScriptableObject
    {
        [Header("Movement")]
        public float walkMaxSpeed;
        public float groundAcceleration;
        public float groundDeceleration;
    }
}
