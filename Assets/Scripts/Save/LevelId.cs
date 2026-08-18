using System.IO;
using UnityEngine.SceneManagement;

namespace Boltz.Save
{
    /// <summary>
    /// Turns a level's position in the build settings into the stable string the save file uses.
    ///
    /// Levels occupy build indices 1 to 6, and the level select screen lays them out in buttons 0 to
    /// 5, so an ordinal is always one less than a build index. Phase 6 replaces this with an
    /// authored level list; until then this keeps the mapping in one place instead of scattered
    /// arithmetic.
    /// </summary>
    public static class LevelId
    {
        /// <summary>Scene name for a build index, or null when the index is outside the build settings.</summary>
        public static string ForBuildIndex(int buildIndex)
        {
            if (buildIndex < 0 || buildIndex >= SceneManager.sceneCountInBuildSettings)
                return null;

            var path = SceneUtility.GetScenePathByBuildIndex(buildIndex);
            return string.IsNullOrEmpty(path) ? null : Path.GetFileNameWithoutExtension(path);
        }

        /// <summary>Scene name for a level select button index.</summary>
        public static string ForOrdinal(int ordinal)
        {
            return ForBuildIndex(ordinal + 1);
        }
    }
}
