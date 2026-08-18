using System;
using System.Collections.Generic;

namespace Boltz.Save
{
    /// <summary>
    /// The whole persisted player profile. Serialized with JsonUtility, so every persisted
    /// member is a public field and the collection is a List rather than a Dictionary.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        /// <summary>Bump when a field changes meaning, then handle it in SaveService.Upgrade.</summary>
        public const int CurrentVersion = 1;

        public int Version = CurrentVersion;

        /// <summary>
        /// One record per level the player has finished. Holding progress per level is what stops
        /// a weak run on one level from overwriting the rating shown for another.
        /// </summary>
        public List<LevelProgress> Levels = new List<LevelProgress>();

        /// <summary>Coins banked across every run, shown on the main menu.</summary>
        public int TotalCoins;

        /// <summary>Index into ChooseBall.BallPool of the skin the player last chose.</summary>
        public int SelectedBallId;

        public bool AudioMuted;

        /// <summary>Set once the owl has played its disappearing sequence on the level select screen.</summary>
        public bool IsOwlDisappearedOnce;

        /// <summary>
        /// Reserved. Lives remaining. Boltz sends the player straight to the GameOver scene today;
        /// the field exists so adding lives later needs no save migration.
        /// </summary>
        public int Lives = -1;

        /// <summary>
        /// Returns the record for <paramref name="levelId"/>, creating it if this is the first time
        /// the level has been finished. Never returns null.
        /// </summary>
        public LevelProgress GetOrCreateLevel(string levelId)
        {
            for (int i = 0; i < Levels.Count; i++)
            {
                if (Levels[i].LevelId == levelId)
                    return Levels[i];
            }

            var progress = new LevelProgress { LevelId = levelId };
            Levels.Add(progress);
            return progress;
        }

        /// <summary>Returns the stored record for <paramref name="levelId"/>, or null if it has never been finished.</summary>
        public LevelProgress FindLevel(string levelId)
        {
            for (int i = 0; i < Levels.Count; i++)
            {
                if (Levels[i].LevelId == levelId)
                    return Levels[i];
            }

            return null;
        }

        public int ClearedCount()
        {
            int count = 0;
            for (int i = 0; i < Levels.Count; i++)
            {
                if (Levels[i].Cleared)
                    count++;
            }

            return count;
        }
    }
}
