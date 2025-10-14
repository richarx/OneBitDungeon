using UnityEngine;

namespace Decor.Bonfire
{
    public class Bonfire : MonoBehaviour
    {
        public static Bonfire instance;

        public Vector3 position => transform.position;
        
        private void Awake()
        {
            instance = this;
        }
    }
}
