using UnityEngine;

namespace Boltz.Levels
{
    /// <summary>
    /// A group of levels presented as one page of the level select screen.
    ///
    /// Arenas hold only levels that exist. Arena 2 currently has one, and the level select screen is
    /// expected to render what it finds rather than assume a fixed five.
    /// </summary>
    [CreateAssetMenu(menuName = "Boltz/Arena Definition", fileName = "ArenaDefinition")]
    public class ArenaDefinition : ScriptableObject
    {
        [SerializeField]
        private string _arenaId;

        [SerializeField]
        private string _displayName;

        [Tooltip("Levels in play order. Only levels that actually exist belong here.")]
        [SerializeField]
        private LevelDefinition[] _levels;

        [Tooltip("Stars needed across earlier arenas before this one opens. Zero means always open.")]
        [SerializeField]
        private int _starsToUnlock;

        public string ArenaId => _arenaId;

        public string DisplayName => string.IsNullOrEmpty(_displayName) ? _arenaId : _displayName;

        public LevelDefinition[] Levels => _levels ?? new LevelDefinition[0];

        public int StarsToUnlock => _starsToUnlock;

        /// <summary>Every star obtainable in this arena, which is three per level rather than a fixed 15.</summary>
        public int MaxStars => Levels.Length * 3;

        private void OnValidate()
        {
#if UNITY_EDITOR
            // Only authored assets are worth complaining about. Tests build these in memory and set
            // their fields one at a time, so an unguarded check warns on every half-built instance.
            if (!UnityEditor.EditorUtility.IsPersistent(this))
                return;
#endif

            if (string.IsNullOrEmpty(_arenaId))
                Debug.LogWarning(name + " has no arena id.", this);

            if (_levels == null)
                return;

            for (int i = 0; i < _levels.Length; i++)
            {
                if (_levels[i] == null)
                    Debug.LogWarning(name + " has an empty level slot at index " + i + ".", this);
            }
        }
    }
}
