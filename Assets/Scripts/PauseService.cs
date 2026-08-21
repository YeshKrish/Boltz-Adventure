using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Something that wants the game clock held at zero.</summary>
public enum PauseReason
{
    /// <summary>The player opened the pause menu.</summary>
    PauseScreen,

    /// <summary>A level is showing its instruction screen before play starts.</summary>
    Instructions
}

/// <summary>
/// The only thing in the project that writes Time.timeScale.
///
/// The clock had eight owners spread over UIManager and GameManager, each setting it directly
/// and none of them aware of the others. The pause menu and a level's instruction screen are
/// separate reasons to be stopped, so dismissing either one started the clock even when the
/// other still wanted it stopped. On top of that the resume button restored the clock and hid
/// the panel without clearing the flag that tracked whether the game was paused, so the next
/// press of the pause button took the resume branch, did nothing visible, and it took two
/// presses to pause again.
///
/// Callers hold and release a reason. The clock runs when nothing is holding it.
/// </summary>
public static class PauseService
{
    private static readonly HashSet<PauseReason> Holds = new HashSet<PauseReason>();

    /// <summary>True while anything is holding the clock.</summary>
    public static bool IsPaused
    {
        get { return Holds.Count > 0; }
    }

    /// <summary>True if this particular reason is holding the clock.</summary>
    public static bool IsHeldBy(PauseReason reason)
    {
        return Holds.Contains(reason);
    }

    /// <summary>Stops the clock on behalf of <paramref name="reason"/>. Holding twice is harmless.</summary>
    public static void Hold(PauseReason reason)
    {
        if (Holds.Add(reason))
        {
            Apply();
        }
    }

    /// <summary>
    /// Drops this reason's hold. The clock only restarts once nothing else is holding it.
    /// Releasing something that was not holding is harmless.
    /// </summary>
    public static void Release(PauseReason reason)
    {
        if (Holds.Remove(reason))
        {
            Apply();
        }
    }

    /// <summary>Drops every hold and starts the clock. Used when leaving a level.</summary>
    public static void ReleaseAll()
    {
        if (Holds.Count == 0)
        {
            return;
        }

        Holds.Clear();
        Apply();
    }

    private static void Apply()
    {
        Time.timeScale = Holds.Count > 0 ? 0f : 1f;
    }

    // Static state outlives both scene loads and, when the editor is set to skip the domain
    // reload, play sessions. A stale hold here would leave a level frozen on entry with no way
    // to tell why, so the holds are cleared when play starts and again on every scene load.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetForPlaySession()
    {
        Holds.Clear();
        Time.timeScale = 1f;

        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ReleaseAll();
        Time.timeScale = 1f;
    }
}
