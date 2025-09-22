using UnityEngine;

namespace Tools_and_Scripts
{
    public static class Initializer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Execute()
        {
            Debug.Log("[Initializer] : initialized persistent object");
            Object.DontDestroyOnLoad(Object.Instantiate(Resources.Load("PERSISTOBJECTS")));
        }
    }
}
