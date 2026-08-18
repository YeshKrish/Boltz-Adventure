using UnityEngine;

namespace Boltz.Save
{
    /// <summary>
    /// The single way the game reads and writes player progress.
    ///
    /// Deliberately a plain static class rather than a MonoBehaviour singleton. The old SaveManager
    /// was a scene object, so anything running before it existed, or in a scene that did not contain
    /// it, dereferenced null. There is no such window here: the first access loads.
    ///
    /// Writes are batched. Callers change state and the profile is flushed on pause and on quit by
    /// <see cref="SaveFlushBehaviour"/>, or immediately via <see cref="Flush"/> at a point where
    /// losing the write would matter.
    /// </summary>
    public static class SaveService
    {
        private static ISaveStore _store;
        private static SaveData _data;
        private static bool _isDirty;

        /// <summary>The loaded profile, loading it on first access. Never null.</summary>
        public static SaveData Data
        {
            get
            {
                EnsureLoaded();
                return _data;
            }
        }

        public static int TotalCoins => Data.TotalCoins;

        public static int ClearedCount => Data.ClearedCount();

        public static int SelectedBallId
        {
            get => Data.SelectedBallId;
            set
            {
                if (Data.SelectedBallId == value)
                    return;

                Data.SelectedBallId = value;
                _isDirty = true;
            }
        }

        public static bool IsOwlDisappearedOnce
        {
            get => Data.IsOwlDisappearedOnce;
            set
            {
                if (Data.IsOwlDisappearedOnce == value)
                    return;

                Data.IsOwlDisappearedOnce = value;
                _isDirty = true;
            }
        }

        public static bool AudioMuted
        {
            get => Data.AudioMuted;
            set
            {
                if (Data.AudioMuted == value)
                    return;

                Data.AudioMuted = value;
                _isDirty = true;
            }
        }

        /// <summary>
        /// Swaps in a different backing store and drops anything already loaded. Tests use this to
        /// inject an in-memory store; nothing in the shipping game calls it.
        /// </summary>
        public static void UseStore(ISaveStore store)
        {
            _store = store;
            _data = null;
            _isDirty = false;
        }

        /// <summary>Best stars earned on a level, 0 if it has never been finished.</summary>
        public static int GetStars(string levelId)
        {
            var progress = Data.FindLevel(levelId);
            return progress == null ? 0 : progress.Stars;
        }

        public static bool IsCleared(string levelId)
        {
            var progress = Data.FindLevel(levelId);
            return progress != null && progress.Cleared;
        }

        /// <summary>
        /// Records the outcome of a run. Stars and coins only ever move up, so replaying a level
        /// badly cannot take away a rating the player already earned.
        /// </summary>
        public static void RecordLevelResult(string levelId, int stars, int coinsCollected)
        {
            if (string.IsNullOrEmpty(levelId))
                return;

            var progress = Data.GetOrCreateLevel(levelId);
            progress.Cleared = true;

            if (stars > progress.Stars)
                progress.Stars = stars;

            if (coinsCollected > progress.BestCoins)
                progress.BestCoins = coinsCollected;

            _isDirty = true;
        }

        /// <summary>Adds to the running coin total shown on the main menu.</summary>
        public static void AddCoins(int amount)
        {
            if (amount == 0)
                return;

            Data.TotalCoins += amount;
            _isDirty = true;
        }

        /// <summary>Total stars earned across the given levels, used to decide when an arena is complete.</summary>
        public static int TotalStars(string[] levelIds)
        {
            if (levelIds == null)
                return 0;

            int total = 0;
            for (int i = 0; i < levelIds.Length; i++)
            {
                total += GetStars(levelIds[i]);
            }

            return total;
        }

        public static void MarkDirty()
        {
            _isDirty = true;
        }

        /// <summary>Writes the profile if anything has changed since the last write.</summary>
        public static void Flush()
        {
            if (!_isDirty || _data == null)
                return;

            EnsureStore();
            _store.Save(_data);
            _isDirty = false;
        }

        /// <summary>Erases the profile and starts again from defaults.</summary>
        public static void DeleteAll()
        {
            EnsureStore();
            _store.Delete();
            _data = new SaveData();
            _isDirty = false;
        }

        /// <summary>
        /// Drops loaded state so the next access reloads.
        ///
        /// Runs on subsystem registration so that entering play mode with domain reload turned off
        /// starts from disk like a real launch, instead of reusing the previous session's object.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Reset()
        {
            _store = null;
            _data = null;
            _isDirty = false;
        }

        private static void EnsureLoaded()
        {
            if (_data != null)
                return;

            EnsureStore();

            // Whether a profile already exists decides if this is a returning player coming from an
            // older build, which is the only time the legacy PlayerPrefs data should be read.
            bool isFirstRunOfThisFormat = !_store.Exists;

            _data = _store.Load();

            if (isFirstRunOfThisFormat && LegacySaveMigrator.TryMigrate(_data))
            {
                _isDirty = true;
                Flush();
            }

            Upgrade(_data);
        }

        private static void EnsureStore()
        {
            if (_store == null)
                _store = new FileSaveStore();
        }

        /// <summary>
        /// Brings an older profile up to the current version. Nothing to do at version 1; the hook
        /// exists so the first real format change has an obvious home.
        /// </summary>
        private static void Upgrade(SaveData data)
        {
            if (data.Version >= SaveData.CurrentVersion)
                return;

            data.Version = SaveData.CurrentVersion;
            _isDirty = true;
        }
    }
}
