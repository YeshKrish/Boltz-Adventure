using UnityEngine;

namespace Boltz.Save
{
    /// <summary>
    /// Owns the application lifecycle hooks that flush the profile to disk.
    ///
    /// Installs itself at startup, so there is nothing to wire into a scene and no chance of a scene
    /// loading without it. Android routinely kills a backgrounded app without ever calling
    /// OnApplicationQuit, so the pause hook is the one that saves most players' progress.
    /// </summary>
    [DisallowMultipleComponent]
    public class SaveFlushBehaviour : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Install()
        {
            var host = new GameObject("[SaveFlush]");
            host.AddComponent<SaveFlushBehaviour>();
            DontDestroyOnLoad(host);
        }

        private void OnApplicationPause(bool isPaused)
        {
            if (isPaused)
                SaveService.Flush();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
                SaveService.Flush();
        }

        private void OnApplicationQuit()
        {
            SaveService.Flush();
        }
    }
}
