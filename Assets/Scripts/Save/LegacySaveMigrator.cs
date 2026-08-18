using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Boltz.Save
{
    /// <summary>
    /// One-shot import of progress written by version 1.1.x, which kept stars in a tab separated
    /// text file and everything else in loose PlayerPrefs keys.
    ///
    /// Every read here is defensive. The old writer appended instead of overwriting, so the star
    /// file has repeated keys and the owl flag file can contain "TrueTrue" or nothing at all. A
    /// player upgrading from the store build must not lose their progress to a parse error, so a
    /// line that cannot be read is skipped rather than allowed to throw.
    /// </summary>
    public static class LegacySaveMigrator
    {
        private const string LegacyStarFileName = "levelAndStar.txt";
        private const string LegacyOwlFileName = "isOwlTriggered.txt";

        // Spelled exactly as the shipped build wrote them, typos included. These strings are baked
        // into real players' devices and cannot be corrected.
        private const string KeyTotalCoins = "CoinsCollectedQuantity";
        private const string KeySelectedBall = "PreviousBall";
        private const string KeyLevelClearedCount = "LevelClearedCount";
        private const string KeyLevelCleared = "LevelCleared";
        private const string KeyCoinsCollected = "CoinsCollected";
        private const string KeyCurrentLevel = "Current Level";
        private const string KeyIsLastSceneMainMenu = "IsLastSceneMainMenu";
        private const string KeyGameOverLevel = "GameOverLevel";
        private const string KeyMainMenuChanged = "IsMainMenuChnagedAtLeastOnce";

        private static readonly string[] AllLegacyKeys =
        {
            KeyTotalCoins, KeySelectedBall, KeyLevelClearedCount, KeyLevelCleared,
            KeyCoinsCollected, KeyCurrentLevel, KeyIsLastSceneMainMenu, KeyGameOverLevel,
            KeyMainMenuChanged
        };

        /// <summary>
        /// Fills <paramref name="data"/> from any version 1.1.x progress found on the device and
        /// clears the legacy PlayerPrefs keys afterwards. Returns true when something was imported,
        /// which is the caller's signal to write the new profile.
        ///
        /// The old star file is left on disk. It is small, and keeping it for one release means a
        /// player who rolls back to the store build does not land on an empty save.
        /// </summary>
        public static bool TryMigrate(SaveData data)
        {
            if (data == null)
                return false;

            bool migratedAnything = false;

            migratedAnything |= TryMigrateStarFile(data);
            migratedAnything |= TryMigrateOwlFlag(data);
            migratedAnything |= TryMigratePlayerPrefs(data);

            if (migratedAnything)
            {
                ClearLegacyKeys();
                Debug.Log("Imported progress from the version 1.1 save format.");
            }

            return migratedAnything;
        }

        /// <summary>
        /// Applies the contents of the legacy star file to <paramref name="data"/>.
        ///
        /// Kept separate from the file reading so it can be tested directly. Each line is
        /// "ordinal\tstars"; the highest star value wins when an ordinal repeats, matching how the
        /// old loader collapsed its appended duplicates.
        /// </summary>
        public static bool ApplyStarLines(SaveData data, IEnumerable<string> lines, Func<int, string> levelIdForOrdinal)
        {
            if (data == null || lines == null || levelIdForOrdinal == null)
                return false;

            var bestStarsByOrdinal = new Dictionary<int, int>();

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var parts = line.Split('\t');
                if (parts.Length < 2)
                    continue;

                if (!int.TryParse(parts[0].Trim(), out int ordinal))
                    continue;

                if (!int.TryParse(parts[1].Trim(), out int stars))
                    continue;

                if (ordinal < 0)
                    continue;

                stars = Mathf.Clamp(stars, 0, 3);

                if (!bestStarsByOrdinal.TryGetValue(ordinal, out int existing) || stars > existing)
                    bestStarsByOrdinal[ordinal] = stars;
            }

            bool applied = false;

            foreach (var pair in bestStarsByOrdinal)
            {
                var levelId = levelIdForOrdinal(pair.Key);
                if (string.IsNullOrEmpty(levelId))
                    continue;

                var progress = data.GetOrCreateLevel(levelId);
                progress.Cleared = true;

                if (pair.Value > progress.Stars)
                    progress.Stars = pair.Value;

                applied = true;
            }

            return applied;
        }

        private static bool TryMigrateStarFile(SaveData data)
        {
            var path = Path.Combine(Application.persistentDataPath, LegacyStarFileName);

            string[] lines;
            try
            {
                if (!File.Exists(path))
                    return false;

                lines = File.ReadAllLines(path);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Could not read legacy star file at {path}: {e.Message}");
                return false;
            }

            return ApplyStarLines(data, lines, LevelId.ForOrdinal);
        }

        private static bool TryMigrateOwlFlag(SaveData data)
        {
            var path = Path.Combine(Application.persistentDataPath, LegacyOwlFileName);

            try
            {
                if (!File.Exists(path))
                    return false;

                // The old writer appended, so this file holds "", "True", or "TrueTrue" and only
                // ever meant one thing: the owl sequence has played at least once.
                var contents = File.ReadAllText(path);
                if (contents.IndexOf("True", StringComparison.OrdinalIgnoreCase) < 0)
                    return false;

                data.IsOwlDisappearedOnce = true;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Could not read legacy owl flag at {path}: {e.Message}");
                return false;
            }
        }

        private static bool TryMigratePlayerPrefs(SaveData data)
        {
            bool migratedAnything = false;

            if (PlayerPrefs.HasKey(KeyTotalCoins))
            {
                data.TotalCoins = Mathf.Max(0, PlayerPrefs.GetInt(KeyTotalCoins));
                migratedAnything = true;
            }

            if (PlayerPrefs.HasKey(KeySelectedBall))
            {
                data.SelectedBallId = Mathf.Max(0, PlayerPrefs.GetInt(KeySelectedBall));
                migratedAnything = true;
            }

            // The star file is the better record of which levels were cleared, because it names
            // them. This only fills in levels the player finished before the star file existed, or
            // finished with too few coins to earn a star.
            if (PlayerPrefs.HasKey(KeyLevelClearedCount))
            {
                int clearedCount = Mathf.Max(0, PlayerPrefs.GetInt(KeyLevelClearedCount));
                for (int ordinal = 0; ordinal < clearedCount; ordinal++)
                {
                    var levelId = LevelId.ForOrdinal(ordinal);
                    if (string.IsNullOrEmpty(levelId))
                        continue;

                    data.GetOrCreateLevel(levelId).Cleared = true;
                }

                migratedAnything |= clearedCount > 0;
            }

            // The remaining legacy keys held session state rather than progress: which level was
            // being played, whether the level select screen was reached from the menu, and a
            // constant. Nothing to carry across, but they are still cleared below.
            return migratedAnything;
        }

        private static void ClearLegacyKeys()
        {
            for (int i = 0; i < AllLegacyKeys.Length; i++)
            {
                PlayerPrefs.DeleteKey(AllLegacyKeys[i]);
            }

            // The shipped game never once called this, which is why a hard kill could lose
            // progress that PlayerPrefs had supposedly stored.
            PlayerPrefs.Save();
        }
    }
}
