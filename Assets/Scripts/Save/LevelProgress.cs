using System;

namespace Boltz.Save
{
    /// <summary>
    /// One level's persisted result. Keyed by scene name rather than build index so that
    /// reordering the build settings does not silently reassign a player's stars.
    /// </summary>
    [Serializable]
    public class LevelProgress
    {
        /// <summary>Scene name, for example "Level1-3".</summary>
        public string LevelId;

        /// <summary>Best star rating earned, 0 to 3.</summary>
        public int Stars;

        /// <summary>Most coins collected in a single run of this level.</summary>
        public int BestCoins;

        public bool Cleared;

        /// <summary>
        /// Reserved. Bitmask of hidden collectibles found in this level, one bit per secret.
        /// Nothing writes this yet; the field exists so adding secrets later needs no save migration.
        /// </summary>
        public int SecretsMask;

        /// <summary>
        /// Reserved. Index of the last checkpoint reached, -1 for none. Boltz has no checkpoints
        /// today; the field exists so adding them later needs no save migration.
        /// </summary>
        public int CheckpointIndex = -1;
    }
}
