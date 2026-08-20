using Boltz.Save;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Boltz.Levels
{
    /// <summary>
    /// Where the game is in the level order, and the single place a level result is written.
    ///
    /// Progress used to be written from two separate paths into one append-only file, which is how a
    /// weak replay could overwrite the rating already shown for a level. Everything now goes through
    /// <see cref="CompleteCurrent"/>.
    /// </summary>
    public static class LevelFlow
    {
        private const string DatabaseResourceName = "LevelDatabase";

        private static LevelDatabase _database;

        /// <summary>
        /// The authored database, loaded from Resources on first use.
        ///
        /// Loaded rather than wired into a scene so that no level scene has to carry a reference and
        /// no execution order decides whether it is ready.
        /// </summary>
        public static LevelDatabase Database
        {
            get
            {
                if (_database == null)
                {
                    _database = Resources.Load<LevelDatabase>(DatabaseResourceName);
                    if (_database == null)
                    {
                        Debug.LogError(
                            "No LevelDatabase found at Resources/" + DatabaseResourceName
                            + ". Level order, star thresholds and the end of the game all depend on it.");
                    }
                }

                return _database;
            }
        }

        /// <summary>The level whose scene is currently active, or null when the active scene is not a level.</summary>
        public static LevelDefinition Current
        {
            get
            {
                var database = Database;
                return database == null ? null : database.GetByScene(SceneManager.GetActiveScene().name);
            }
        }

        /// <summary>Points the flow at a database directly. For tests, which have no Resources folder.</summary>
        public static void UseDatabase(LevelDatabase database)
        {
            _database = database;
        }

        /// <summary>
        /// Scores and stores the run that just finished, and reports whether it was the last level.
        ///
        /// Writing immediately rather than waiting for a pause or quit, because finishing a level is
        /// the one moment a player would be angry to lose.
        /// </summary>
        public static bool CompleteCurrent(int coinsCollected)
        {
            var level = Current;
            if (level == null)
            {
                Debug.LogError(
                    "Scene '" + SceneManager.GetActiveScene().name + "' finished but is not in the level "
                    + "database, so the result was not saved.");
                return false;
            }

            int stars = level.StarsFor(coinsCollected);

            SaveService.AddCoins(coinsCollected);
            SaveService.RecordLevelResult(level.LevelId, stars, coinsCollected);

            // The level select screen reads these on the way in to decide which stars to pop.
            GameSession.LastRunLevelId = level.LevelId;
            GameSession.LastRunStars = stars;

            SaveService.Flush();

            var database = Database;
            return database != null && database.IsFinal(level);
        }

        /// <summary>Clears the cached database so a new play session reloads it.</summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Reset()
        {
            _database = null;
        }
    }
}
