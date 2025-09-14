using UnityEngine;

namespace Level_Holder
{
    public class DestroyOnResetGame : MonoBehaviour
    {
        private void Start()
        {
            LevelHolder.OnResetGame.AddListener(() => Destroy(gameObject));
        }
    }
}
