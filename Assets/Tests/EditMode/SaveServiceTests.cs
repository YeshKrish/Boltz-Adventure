using Boltz.Save;
using NUnit.Framework;

namespace Boltz.Tests.EditMode
{
    public class SaveServiceTests
    {
        private MemorySaveStore _store;

        [SetUp]
        public void SetUp()
        {
            _store = new MemorySaveStore();
            SaveService.UseStore(_store);
        }

        [TearDown]
        public void TearDown()
        {
            SaveService.Reset();
        }

        [Test]
        public void GetStars_LevelNeverPlayed_IsZero()
        {
            Assert.AreEqual(0, SaveService.GetStars("Level1-1"));
            Assert.IsFalse(SaveService.IsCleared("Level1-1"));
        }

        [Test]
        public void RecordLevelResult_StoresStarsAndMarksCleared()
        {
            SaveService.RecordLevelResult("Level1-1", 2, 7);

            Assert.AreEqual(2, SaveService.GetStars("Level1-1"));
            Assert.IsTrue(SaveService.IsCleared("Level1-1"));
        }

        [Test]
        public void RecordLevelResult_WorseReplay_KeepsTheBetterResult()
        {
            SaveService.RecordLevelResult("Level1-1", 3, 12);
            SaveService.RecordLevelResult("Level1-1", 1, 3);

            Assert.AreEqual(3, SaveService.GetStars("Level1-1"), "Stars must never go down on a replay.");
            Assert.AreEqual(12, SaveService.Data.FindLevel("Level1-1").BestCoins);
        }

        /// <summary>
        /// The defect in section 8 of the audit. Stars used to live in one global PlayerPrefs string,
        /// so finishing any level with too few coins to score left the previous level's rating in
        /// place, and the level select screen drew it against the wrong level. Records are per level
        /// now, which makes the situation unrepresentable rather than merely corrected.
        /// </summary>
        [Test]
        public void RecordLevelResult_DoesNotLeakBetweenLevels()
        {
            SaveService.RecordLevelResult("Level1-1", 3, 12);
            SaveService.RecordLevelResult("Level1-2", 0, 1);

            Assert.AreEqual(3, SaveService.GetStars("Level1-1"));
            Assert.AreEqual(0, SaveService.GetStars("Level1-2"));
        }

        [Test]
        public void ClearedCount_CountsDistinctLevels()
        {
            SaveService.RecordLevelResult("Level1-1", 3, 12);
            SaveService.RecordLevelResult("Level1-1", 2, 8);
            SaveService.RecordLevelResult("Level1-2", 1, 4);

            Assert.AreEqual(2, SaveService.ClearedCount);
        }

        [Test]
        public void AddCoins_Accumulates()
        {
            SaveService.AddCoins(10);
            SaveService.AddCoins(5);

            Assert.AreEqual(15, SaveService.TotalCoins);
        }

        [Test]
        public void TotalStars_SumsTheGivenLevels()
        {
            SaveService.RecordLevelResult("Level1-1", 3, 12);
            SaveService.RecordLevelResult("Level1-2", 2, 8);
            SaveService.RecordLevelResult("Level2-1", 3, 9);

            Assert.AreEqual(5, SaveService.TotalStars(new[] { "Level1-1", "Level1-2" }));
        }

        [Test]
        public void Flush_WritesOnlyWhenSomethingChanged()
        {
            SaveService.RecordLevelResult("Level1-1", 3, 12);
            SaveService.Flush();
            Assert.AreEqual(1, _store.SaveCount);

            SaveService.Flush();
            Assert.AreEqual(1, _store.SaveCount, "A flush with no pending changes must not rewrite the file.");
        }

        [Test]
        public void Flush_ThenReload_KeepsProgress()
        {
            SaveService.RecordLevelResult("Level1-3", 2, 6);
            SaveService.SelectedBallId = 2;
            SaveService.IsOwlDisappearedOnce = true;
            SaveService.AddCoins(40);
            SaveService.Flush();

            // Same store, fresh service state: this is what relaunching the game does.
            SaveService.UseStore(_store);

            Assert.AreEqual(2, SaveService.GetStars("Level1-3"));
            Assert.AreEqual(2, SaveService.SelectedBallId);
            Assert.IsTrue(SaveService.IsOwlDisappearedOnce);
            Assert.AreEqual(40, SaveService.TotalCoins);
        }

        [Test]
        public void DeleteAll_ReturnsToDefaults()
        {
            SaveService.RecordLevelResult("Level1-1", 3, 12);
            SaveService.AddCoins(99);

            SaveService.DeleteAll();

            Assert.AreEqual(0, SaveService.TotalCoins);
            Assert.AreEqual(0, SaveService.ClearedCount);
            Assert.AreEqual(0, SaveService.GetStars("Level1-1"));
        }

        [Test]
        public void Data_IsNeverNull()
        {
            SaveService.UseStore(new MemorySaveStore("this is not json"));

            Assert.IsNotNull(SaveService.Data);
            Assert.IsNotNull(SaveService.Data.Levels);
        }
    }
}
