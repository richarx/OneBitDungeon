using UnityEngine;

namespace Decor.Grass
{
    public class GrassVelocityController : MonoBehaviour
    {
        public float externalInfluenceStrength;
        public float easeInTime;
        public float easeOutTime;
        public float velocityThreshold;

        private int externalInfluenceProperty = Shader.PropertyToID("_ExternalInfluence");

        public void ApplyInfluence(Material material, float xVelocity)
        {
            material.SetFloat(externalInfluenceProperty, xVelocity);
        }
    }
}
