using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tools_and_Scripts
{
    public static class Initializer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Execute()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (scene.name.Contains("MainMenu") || scene.name.Contains("Intro"))
            {
                Debug.Log("[Initializer] : excluded scene detected - no loading done");
                return;
            }

            Debug.Log("[Initializer] : initialized persistent object");
            Object.DontDestroyOnLoad(Object.Instantiate(Resources.Load("PERSISTOBJECTS")));
        }
    }
}
