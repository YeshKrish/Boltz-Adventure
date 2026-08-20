using UnityEngine;

namespace Boltz.Levels
{
    /// <summary>
    /// Everything the game needs to know about one playable level.
    ///
    /// The level's identity and its scene are deliberately two separate fields. <see cref="LevelId"/>
    /// is the key written into the save profile and must never change once a build has shipped with
    /// it, because renaming it orphans every player's stars for that level. The scene may be renamed
    /// or moved freely.
    /// </summary>
    [CreateAssetMenu(menuName = "Boltz/Level Definition", fileName = "LevelDefinition")]
    public class LevelDefinition : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Save key. Never change this once a build has shipped, it orphans existing progress.")]
        [SerializeField]
        private string _levelId;

        [Tooltip("Scene to load. Safe to rename, unlike the level id.")]
        [SerializeField]
        private string _sceneName;

        [SerializeField]
        private string _displayName;

        [Header("Scoring")]
        [Tooltip("How many coins the level contains. Should match the CoinBag child count in the scene.")]
        [SerializeField]
        private int _totalCoins;

        [Tooltip("Absolute coin counts needed for one, two and three stars. Not ratios.")]
        [SerializeField]
        private Vector3Int _starThresholds;

        [Header("Presentation")]
        [Tooltip("Freezes time and hides the HUD on entry, for the instruction overlay.")]
        [SerializeField]
        private bool _showsIntro;

        public string LevelId => _levelId;

        public string SceneName => _sceneName;

        public string DisplayName => string.IsNullOrEmpty(_displayName) ? _levelId : _displayName;

        public int TotalCoins => _totalCoins;

        public Vector3Int StarThresholds => _starThresholds;

        public bool ShowsIntro => _showsIntro;

        /// <summary>
        /// Stars awarded for a run that collected <paramref name="coinsCollected"/> coins.
        ///
        /// The thresholds are absolute counts, which is the whole point of authoring them. The
        /// previous version divided the total at runtime and got the rounding wrong, so both the
        /// two and one star thresholds could sit a coin below where they were meant to.
        /// </summary>
        public int StarsFor(int coinsCollected)
        {
            if (_totalCoins <= 0)
                return 0;

            if (coinsCollected >= _starThresholds.z)
                return 3;

            if (coinsCollected >= _starThresholds.y)
                return 2;

            if (coinsCollected >= _starThresholds.x)
                return 1;

            return 0;
        }

        private void OnValidate()
        {
#if UNITY_EDITOR
            // Only authored assets are worth complaining about. Tests build these in memory and set
            // their fields one at a time, so an unguarded check warns on every half-built instance.
            if (!UnityEditor.EditorUtility.IsPersistent(this))
                return;
#endif

            if (string.IsNullOrEmpty(_levelId))
                Debug.LogWarning(name + " has no level id, so nothing it scores can be saved.", this);

            if (string.IsNullOrEmpty(_sceneName))
                Debug.LogWarning(name + " has no scene name, so it cannot be loaded.", this);

            var t = _starThresholds;
            if (t.x <= 0 || t.x > t.y || t.y > t.z || t.z > _totalCoins)
            {
                Debug.LogWarning(
                    name + " has star thresholds " + t + " that do not satisfy 0 < one <= two <= three <= "
                    + _totalCoins + ", so some ratings are unreachable.", this);
            }
        }
    }
}
