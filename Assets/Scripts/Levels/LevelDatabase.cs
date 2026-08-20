using System.Collections.Generic;
using UnityEngine;

namespace Boltz.Levels
{
    /// <summary>
    /// The authored list of arenas and levels, and the only place that knows what order they run in.
    ///
    /// Replaces arithmetic on build indices. Build order is a project setting that reorders whenever
    /// somebody drags a scene in the build settings window, and the old code derived the next level,
    /// the final level and the level select button mapping from it.
    /// </summary>
    [CreateAssetMenu(menuName = "Boltz/Level Database", fileName = "LevelDatabase")]
    public class LevelDatabase : ScriptableObject
    {
        [SerializeField]
        private ArenaDefinition[] _arenas;

        private List<LevelDefinition> _all;

        public ArenaDefinition[] Arenas => _arenas ?? new ArenaDefinition[0];

        /// <summary>Every level across every arena, in play order.</summary>
        public IReadOnlyList<LevelDefinition> All
        {
            get
            {
                if (_all == null)
                    Rebuild();

                return _all;
            }
        }

        public LevelDefinition GetById(string levelId)
        {
            if (string.IsNullOrEmpty(levelId))
                return null;

            var all = All;
            for (int i = 0; i < all.Count; i++)
            {
                if (all[i].LevelId == levelId)
                    return all[i];
            }

            return null;
        }

        public LevelDefinition GetByScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
                return null;

            var all = All;
            for (int i = 0; i < all.Count; i++)
            {
                if (all[i].SceneName == sceneName)
                    return all[i];
            }

            return null;
        }

        /// <summary>The level after <paramref name="level"/>, or null when it is the last one.</summary>
        public LevelDefinition Next(LevelDefinition level)
        {
            int i = IndexOf(level);
            if (i < 0 || i + 1 >= All.Count)
                return null;

            return All[i + 1];
        }

        public bool IsFinal(LevelDefinition level)
        {
            int i = IndexOf(level);
            return i >= 0 && i == All.Count - 1;
        }

        public int IndexOf(LevelDefinition level)
        {
            if (level == null)
                return -1;

            var all = All;
            for (int i = 0; i < all.Count; i++)
            {
                if (all[i] == level)
                    return i;
            }

            return -1;
        }

        /// <summary>The arena containing <paramref name="level"/>, or null.</summary>
        public ArenaDefinition ArenaOf(LevelDefinition level)
        {
            if (level == null)
                return null;

            foreach (var arena in Arenas)
            {
                if (arena == null)
                    continue;

                foreach (var candidate in arena.Levels)
                {
                    if (candidate == level)
                        return arena;
                }
            }

            return null;
        }

        private void Rebuild()
        {
            _all = new List<LevelDefinition>();
            foreach (var arena in Arenas)
            {
                if (arena == null)
                    continue;

                foreach (var level in arena.Levels)
                {
                    if (level != null)
                        _all.Add(level);
                }
            }
        }

        private void OnValidate()
        {
            // Always drop the cache. Editing the arena list is exactly when it goes stale, and this
            // has to happen for in-memory instances too.
            _all = null;

#if UNITY_EDITOR
            // Only authored assets are worth complaining about. Tests build these in memory and set
            // their fields one at a time, so an unguarded check warns on every half-built instance.
            if (!UnityEditor.EditorUtility.IsPersistent(this))
                return;
#endif

            var seen = new HashSet<string>();
            foreach (var level in All)
            {
                if (!seen.Add(level.LevelId))
                {
                    Debug.LogWarning(
                        "Level id '" + level.LevelId + "' appears more than once, so saved progress for "
                        + "those levels will collide.", this);
                }
            }
        }
    }
}
