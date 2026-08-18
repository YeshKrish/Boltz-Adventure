using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Boltz.Save
{
    /// <summary>
    /// Stores the profile as JSON in persistentDataPath.
    ///
    /// Writes go to a temporary file first and are then swapped in, so a process death partway
    /// through a write leaves the previous save intact rather than a half-written one. The
    /// displaced file is kept as a backup and used if the main file is ever unreadable.
    /// </summary>
    public class FileSaveStore : ISaveStore
    {
        private const string FileName = "boltz.save.json";

        private readonly string _path;
        private readonly string _backupPath;
        private readonly string _tempPath;

        /// <summary>Uses <see cref="Application.persistentDataPath"/>. This is the shipping configuration.</summary>
        public FileSaveStore() : this(Application.persistentDataPath)
        {
        }

        /// <summary>Uses an explicit directory. Tests pass a temporary folder.</summary>
        public FileSaveStore(string directory)
        {
            _path = Path.Combine(directory, FileName);
            _backupPath = _path + ".bak";
            _tempPath = _path + ".tmp";
        }

        public bool Exists => File.Exists(_path) || File.Exists(_backupPath);

        public SaveData Load()
        {
            if (TryReadFrom(_path, out var data))
                return data;

            if (TryReadFrom(_backupPath, out data))
            {
                Debug.LogWarning($"Save file at {_path} was unreadable, recovered from backup.");
                return data;
            }

            return new SaveData();
        }

        public void Save(SaveData data)
        {
            if (data == null)
                return;

            try
            {
                var json = JsonUtility.ToJson(data);
                File.WriteAllText(_tempPath, json);

                if (File.Exists(_path))
                {
                    // Replace keeps the displaced file as our backup and swaps atomically where
                    // the filesystem supports it.
                    File.Replace(_tempPath, _path, _backupPath);
                }
                else
                {
                    File.Move(_tempPath, _path);
                }
            }
            catch (Exception e)
            {
                // A failed save must not take the game down with it. Fall back to a plain
                // overwrite, which is not atomic but is better than losing the write entirely.
                Debug.LogWarning($"Atomic save to {_path} failed ({e.Message}), falling back to a direct write.");
                TryDirectWrite(data);
            }
        }

        public void Delete()
        {
            TryDelete(_path);
            TryDelete(_backupPath);
            TryDelete(_tempPath);
        }

        private bool TryReadFrom(string path, out SaveData data)
        {
            data = null;

            try
            {
                if (!File.Exists(path))
                    return false;

                var json = File.ReadAllText(path);
                if (string.IsNullOrWhiteSpace(json))
                    return false;

                data = JsonUtility.FromJson<SaveData>(json);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Could not read save at {path}: {e.Message}");
                return false;
            }

            if (data == null)
                return false;

            // JsonUtility leaves a collection null when the field is absent from the payload,
            // so every caller downstream would have to null-check. Normalise here instead.
            if (data.Levels == null)
                data.Levels = new List<LevelProgress>();

            return true;
        }

        private void TryDirectWrite(SaveData data)
        {
            try
            {
                File.WriteAllText(_path, JsonUtility.ToJson(data));
            }
            catch (Exception e)
            {
                Debug.LogError($"Could not write save to {_path}: {e.Message}");
            }
        }

        private static void TryDelete(string path)
        {
            try
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Could not delete {path}: {e.Message}");
            }
        }
    }
}
