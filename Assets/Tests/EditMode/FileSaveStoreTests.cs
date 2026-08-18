using System;
using System.IO;
using Boltz.Save;
using NUnit.Framework;

namespace Boltz.Tests.EditMode
{
    /// <summary>
    /// Covers the failure modes that made the old save system lossy: a missing file, a file that
    /// cannot be parsed, and a write that lands on top of an existing profile.
    /// </summary>
    public class FileSaveStoreTests
    {
        private string _directory;
        private FileSaveStore _store;

        private string MainPath => Path.Combine(_directory, "boltz.save.json");
        private string BackupPath => MainPath + ".bak";

        [SetUp]
        public void SetUp()
        {
            _directory = Path.Combine(Path.GetTempPath(), "boltz-save-tests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_directory);
            _store = new FileSaveStore(_directory);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_directory))
                Directory.Delete(_directory, true);
        }

        [Test]
        public void Exists_BeforeAnyWrite_IsFalse()
        {
            Assert.IsFalse(_store.Exists);
        }

        [Test]
        public void Load_WithNoFile_ReturnsDefaultsWithoutThrowing()
        {
            var data = _store.Load();

            Assert.IsNotNull(data);
            Assert.IsNotNull(data.Levels);
            Assert.AreEqual(0, data.Levels.Count);
            Assert.AreEqual(0, data.TotalCoins);
        }

        [Test]
        public void SaveThenLoad_RoundTripsEveryField()
        {
            var data = new SaveData { TotalCoins = 120, SelectedBallId = 3, IsOwlDisappearedOnce = true };
            data.GetOrCreateLevel("Level1-2").Stars = 2;
            data.GetOrCreateLevel("Level1-2").Cleared = true;

            _store.Save(data);
            var loaded = _store.Load();

            Assert.AreEqual(120, loaded.TotalCoins);
            Assert.AreEqual(3, loaded.SelectedBallId);
            Assert.IsTrue(loaded.IsOwlDisappearedOnce);
            Assert.AreEqual(2, loaded.FindLevel("Level1-2").Stars);
            Assert.IsTrue(loaded.FindLevel("Level1-2").Cleared);
        }

        [Test]
        public void Save_DoesNotLeaveATempFileBehind()
        {
            _store.Save(new SaveData { TotalCoins = 1 });

            Assert.IsFalse(File.Exists(MainPath + ".tmp"));
        }

        [Test]
        public void Load_WithCorruptFile_FallsBackToTheBackup()
        {
            _store.Save(new SaveData { TotalCoins = 100 });
            // The second write displaces the first, which becomes the backup.
            _store.Save(new SaveData { TotalCoins = 200 });
            Assert.IsTrue(File.Exists(BackupPath), "A second write should leave a backup behind.");

            File.WriteAllText(MainPath, "{ this is not valid json");

            var loaded = _store.Load();

            Assert.AreEqual(100, loaded.TotalCoins, "Should have recovered the previous profile.");
        }

        [Test]
        public void Load_WithCorruptFileAndNoBackup_ReturnsDefaults()
        {
            _store.Save(new SaveData { TotalCoins = 100 });
            File.WriteAllText(MainPath, "not json at all");

            var loaded = _store.Load();

            Assert.IsNotNull(loaded);
            Assert.AreEqual(0, loaded.TotalCoins);
        }

        [Test]
        public void Load_WithEmptyFile_ReturnsDefaults()
        {
            File.WriteAllText(MainPath, string.Empty);

            var loaded = _store.Load();

            Assert.IsNotNull(loaded);
            Assert.AreEqual(0, loaded.TotalCoins);
        }

        [Test]
        public void Delete_RemovesEveryFile()
        {
            _store.Save(new SaveData { TotalCoins = 1 });
            _store.Save(new SaveData { TotalCoins = 2 });

            _store.Delete();

            Assert.IsFalse(File.Exists(MainPath));
            Assert.IsFalse(File.Exists(BackupPath));
            Assert.IsFalse(_store.Exists);
        }

        /// <summary>
        /// The old writer used File.AppendAllText for both of its write paths, so the file grew on
        /// every save and keys repeated. Rewriting the same profile must leave one document.
        /// </summary>
        [Test]
        public void RepeatedSaves_DoNotGrowTheFile()
        {
            for (int i = 0; i < 10; i++)
            {
                _store.Save(new SaveData { TotalCoins = i });
            }

            var text = File.ReadAllText(MainPath);

            Assert.AreEqual(1, CountOccurrences(text, "\"TotalCoins\""));
            Assert.AreEqual(9, _store.Load().TotalCoins);
        }

        private static int CountOccurrences(string haystack, string needle)
        {
            int count = 0;
            int index = haystack.IndexOf(needle, StringComparison.Ordinal);
            while (index >= 0)
            {
                count++;
                index = haystack.IndexOf(needle, index + needle.Length, StringComparison.Ordinal);
            }

            return count;
        }
    }
}
