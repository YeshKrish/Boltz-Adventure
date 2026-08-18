using UnityEngine;

namespace Boltz.Save
{
    /// <summary>
    /// State that only matters for the current run of the app.
    ///
    /// Most of this used to live in PlayerPrefs alongside real progress, which made it hard to see
    /// what was actually worth persisting and meant a value like the current level survived a
    /// reinstall for no reason. Nothing here is written to disk.
    /// </summary>
    public static class GameSession
    {
        /// <summary>Build index that follows the last playable level. Reaching it means the game is finished.</summary>
        public const int GameCompletedBuildIndex = 7;

        /// <summary>Build index of the level being played.</summary>
        public static int CurrentLevelBuildIndex { get; set; }

        /// <summary>
        /// True when the level select screen was opened from the main menu rather than by finishing
        /// a level. The screen only plays its star and unlock animations in the second case.
        /// </summary>
        public static bool CameFromMainMenu { get; set; }

        /// <summary>Coins picked up in the level currently being played.</summary>
        public static int CoinsThisLevel { get; set; }

        /// <summary>
        /// Stars earned by the run that just finished, 0 to 3. The level select screen reads this to
        /// decide which stars to pop, then clears it.
        /// </summary>
        public static int LastRunStars { get; set; }

        /// <summary>Scene name of the level that just finished, or null.</summary>
        public static string LastRunLevelId { get; set; }

        /// <summary>
        /// Set once the player has left the main menu this run. The menu's logo and board animations
        /// only play before that, so this deliberately does not persist between runs.
        /// </summary>
        public static bool HasLeftMainMenu { get; set; }

        /// <summary>
        /// Clears everything back to a cold-start state.
        ///
        /// Runs automatically on subsystem registration so that entering play mode with domain
        /// reload turned off behaves the same as a real launch, rather than inheriting values from
        /// the previous play session.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Reset()
        {
            CurrentLevelBuildIndex = 0;
            CameFromMainMenu = false;
            CoinsThisLevel = 0;
            LastRunStars = 0;
            LastRunLevelId = null;
            HasLeftMainMenu = false;
        }
    }
}
