using Boltz.Save;
using UnityEngine;

namespace Boltz.Tests.EditMode
{
    /// <summary>
    /// A save store that keeps the profile in a string instead of on disk, so tests exercise the
    /// real serialization path without touching the developer's persistentDataPath.
    ///
    /// Reports itself as already existing by default. That matters because SaveService only runs
    /// the legacy migration when no profile has ever been written, and a test must never be able to
    /// trip that path and start reading the editor's real PlayerPrefs.
    /// </summary>
    public class MemorySaveStore : ISaveStore
    {
        private string _payload;

        public int SaveCount { get; private set; }

        public MemorySaveStore()
        {
            _payload = JsonUtility.ToJson(new SaveData());
        }

        /// <summary>Starts with the given profile already written.</summary>
        public MemorySaveStore(SaveData seed)
        {
            _payload = seed == null ? null : JsonUtility.ToJson(seed);
        }

        /// <summary>Starts from raw JSON, so a test can hand in something deliberately malformed.</summary>
        public MemorySaveStore(string rawPayload)
        {
            _payload = rawPayload;
        }

        public bool Exists => _payload != null;

        public SaveData Load()
        {
            if (string.IsNullOrEmpty(_payload))
                return new SaveData();

            SaveData data = null;
            try
            {
                data = JsonUtility.FromJson<SaveData>(_payload);
            }
            catch
            {
                // Matches FileSaveStore: an unreadable profile becomes defaults, never an exception.
            }

            return data ?? new SaveData();
        }

        public void Save(SaveData data)
        {
            _payload = JsonUtility.ToJson(data);
            SaveCount++;
        }

        public void Delete()
        {
            _payload = null;
        }
    }
}
