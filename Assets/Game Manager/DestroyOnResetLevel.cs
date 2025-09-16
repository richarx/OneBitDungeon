using UnityEngine;

namespace Game_Manager
{
    public class DestroyOnResetLevel : MonoBehaviour
    {
        private void Start()
        {
            GameManager.OnResetLevel.AddListener(() => Destroy(gameObject));
        }
    }
}
