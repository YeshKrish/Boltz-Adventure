using Boltz.Levels;
using Boltz.Save;
using NUnit.Framework;
using UnityEngine;

namespace Boltz.Tests.EditMode
{
    /// <summary>
    /// The rule that decides which tiles the level select screen opens.
    ///
    /// Worth testing directly because the version this replaced could disagree with itself: it
    /// summed a sliding six level window against a hardcoded fifteen, so once every level was
    /// cleared "arena 1 fully starred" could be satisfied by levels 1-2 through 2-1.
    /// </summary>
    public class LevelProgressionTests
    {
        private LevelDatabase _database;
        private MemorySaveStore _store;

        private static LevelDefinition Level(string id)
        {
            var def = ScriptableObject.CreateInstance<LevelDefinition>();
            var so = new UnityEditor.SerializedObject(def);
            so.FindProperty("_levelId").stringValue = id;
            so.FindProperty("_sceneName").stringValue = id;
            so.ApplyModifiedPropertiesWithoutUndo();
            return def;
        }

        private static ArenaDefinition Arena(string id, int starsToUnlock, params LevelDefinition[] levels)
        {
            var arena = ScriptableObject.CreateInstance<ArenaDefinition>();
            var so = new UnityEditor.SerializedObject(arena);
            so.FindProperty("_arenaId").stringValue = id;
            so.FindProperty("_starsToUnlock").intValue = starsToUnlock;
            var array = so.FindProperty("_levels");
            array.arraySize = levels.Length;
            for (int i = 0; i < levels.Length; i++)
                array.GetArrayElementAtIndex(i).objectReferenceValue = levels[i];
            so.ApplyModifiedPropertiesWithoutUndo();
            return arena;
        }

        [SetUp]
        public void SetUp()
        {
            _store = new MemorySaveStore();
            SaveService.UseStore(_store);

            // Mirrors the shipping shape: five levels in arena 1, one in arena 2 behind all 15 stars.
            var a = new[] { Level("A1"), Level("A2"), Level("A3"), Level("A4"), Level("A5") };
            var b = Level("B1");

            _database = ScriptableObject.CreateInstance<LevelDatabase>();
            var so = new UnityEditor.SerializedObject(_database);
            var arenas = so.FindProperty("_arenas");
            arenas.arraySize = 2;
            arenas.GetArrayElementAtIndex(0).objectReferenceValue = Arena("Arena1", 0, a);
            arenas.GetArrayElementAtIndex(1).objectReferenceValue = Arena("Arena2", 15, b);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        [TearDown]
        public void TearDown()
        {
            SaveService.Reset();
        }

        private void Clear(string levelId, int stars)
        {
            SaveService.RecordLevelResult(levelId, stars, 0);
        }

        [Test]
        public void FirstLevel_IsOpenOnAFreshProfile()
        {
            Assert.IsTrue(LevelProgression.IsUnlocked(_database, 0));
        }

        [Test]
        public void SecondLevel_IsShutUntilTheFirstIsCleared()
        {
            Assert.IsFalse(LevelProgression.IsUnlocked(_database, 1));

            Clear("A1", 1);

            Assert.IsTrue(LevelProgression.IsUnlocked(_database, 1));
        }

        [Test]
        public void ClearingOutOfOrderDoesNotSkipAhead()
        {
            Clear("A3", 3);

            Assert.IsFalse(LevelProgression.IsUnlocked(_database, 3));
        }

        [Test]
        public void ArenaTwo_StaysShutWhenArenaOneIsClearedWithoutEveryStar()
        {
            foreach (var id in new[] { "A1", "A2", "A3", "A4", "A5" })
                Clear(id, 2);

            Assert.IsFalse(LevelProgression.IsUnlocked(_database, 5), "14 stars should not open arena 2");
        }

        [Test]
        public void ArenaTwo_OpensOnAllFifteenStars()
        {
            foreach (var id in new[] { "A1", "A2", "A3", "A4", "A5" })
                Clear(id, 3);

            Assert.IsTrue(LevelProgression.IsUnlocked(_database, 5));
        }

        [Test]
        public void ArenaTwo_IgnoresItsOwnStarsWhenCheckingItsGate()
        {
            // The old sliding window counted the arena 2 level towards the total that unlocks
            // arena 2, which is circular. Only the arenas ahead of it should count.
            foreach (var id in new[] { "A1", "A2", "A3", "A4" })
                Clear(id, 3);
            Clear("A5", 0);
            Clear("B1", 3);

            Assert.AreEqual(12, LevelProgression.StarsBefore(_database, _database.Arenas[1]));
            Assert.IsFalse(LevelProgression.IsUnlocked(_database, 5));
        }

        [Test]
        public void IndexOutsideTheDatabase_IsNeverOpen()
        {
            foreach (var id in new[] { "A1", "A2", "A3", "A4", "A5" })
                Clear(id, 3);
            Clear("B1", 3);

            // The four unbuilt arena 2 slots. Previously these were wired to GameOver, LevelSelect,
            // GameCompleted and Customize, and the screen would unlock the first of them.
            Assert.IsFalse(LevelProgression.IsUnlocked(_database, 6));
            Assert.IsFalse(LevelProgression.IsUnlocked(_database, 9));
            Assert.IsFalse(LevelProgression.IsUnlocked(_database, -1));
        }

        [Test]
        public void NullDatabase_IsNeverOpen()
        {
            Assert.IsFalse(LevelProgression.IsUnlocked(null, 0));
            Assert.AreEqual(0, LevelProgression.StarsBefore(null, null));
        }
    }
}
