using Boltz.Levels;
using NUnit.Framework;
using UnityEngine;

namespace Boltz.Tests.EditMode
{
    public class LevelDefinitionTests
    {
        private static LevelDefinition Level(int totalCoins, int one, int two, int three)
        {
            var def = ScriptableObject.CreateInstance<LevelDefinition>();
            var so = new UnityEditor.SerializedObject(def);
            so.FindProperty("_levelId").stringValue = "Test";
            so.FindProperty("_sceneName").stringValue = "Test";
            so.FindProperty("_totalCoins").intValue = totalCoins;
            so.FindProperty("_starThresholds").vector3IntValue = new Vector3Int(one, two, three);
            so.ApplyModifiedPropertiesWithoutUndo();
            return def;
        }

        [TestCase(0, 0)]
        [TestCase(3, 0)]
        [TestCase(4, 1)]
        [TestCase(7, 1)]
        [TestCase(8, 2)]
        [TestCase(14, 2)]
        [TestCase(15, 3)]
        public void StarsFor_AwardsAtTheAuthoredThresholds(int coins, int expected)
        {
            var level = Level(15, 4, 8, 15);

            Assert.AreEqual(expected, level.StarsFor(coins));
        }

        [Test]
        public void StarsFor_MoreCoinsThanTheLevelHolds_StillCapsAtThree()
        {
            var level = Level(15, 4, 8, 15);

            Assert.AreEqual(3, level.StarsFor(99));
        }

        [Test]
        public void StarsFor_LevelWithNoCoins_AwardsNothing()
        {
            // Guards the case the old code covered with "if (total <= 0) return 0". Without it a
            // run collecting nothing clears a zero threshold and scores three stars.
            var level = Level(0, 0, 0, 0);

            Assert.AreEqual(0, level.StarsFor(0));
        }
    }

    public class LevelDatabaseTests
    {
        private LevelDatabase _database;
        private LevelDefinition _first;
        private LevelDefinition _middle;
        private LevelDefinition _last;

        private static LevelDefinition Level(string id, string scene)
        {
            var def = ScriptableObject.CreateInstance<LevelDefinition>();
            var so = new UnityEditor.SerializedObject(def);
            so.FindProperty("_levelId").stringValue = id;
            so.FindProperty("_sceneName").stringValue = scene;
            so.ApplyModifiedPropertiesWithoutUndo();
            return def;
        }

        private static ArenaDefinition Arena(string id, params LevelDefinition[] levels)
        {
            var arena = ScriptableObject.CreateInstance<ArenaDefinition>();
            var so = new UnityEditor.SerializedObject(arena);
            so.FindProperty("_arenaId").stringValue = id;
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
            _first = Level("A1", "SceneA1");
            _middle = Level("A2", "SceneA2");
            _last = Level("B1", "SceneB1");

            _database = ScriptableObject.CreateInstance<LevelDatabase>();
            var so = new UnityEditor.SerializedObject(_database);
            var arenas = so.FindProperty("_arenas");
            arenas.arraySize = 2;
            arenas.GetArrayElementAtIndex(0).objectReferenceValue = Arena("Arena1", _first, _middle);
            arenas.GetArrayElementAtIndex(1).objectReferenceValue = Arena("Arena2", _last);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        [Test]
        public void All_FlattensArenasInPlayOrder()
        {
            Assert.AreEqual(new[] { _first, _middle, _last }, _database.All);
        }

        [Test]
        public void GetById_FindsALevel()
        {
            Assert.AreSame(_middle, _database.GetById("A2"));
        }

        [Test]
        public void GetById_UnknownId_ReturnsNull()
        {
            Assert.IsNull(_database.GetById("nope"));
            Assert.IsNull(_database.GetById(null));
        }

        [Test]
        public void GetByScene_FindsALevel()
        {
            Assert.AreSame(_last, _database.GetByScene("SceneB1"));
        }

        [Test]
        public void Next_CrossesAnArenaBoundary()
        {
            // The old code added one to the build index, which only worked while every level sat
            // next to its successor in the build settings.
            Assert.AreSame(_last, _database.Next(_middle));
        }

        [Test]
        public void Next_OnTheLastLevel_IsNull()
        {
            Assert.IsNull(_database.Next(_last));
        }

        [Test]
        public void IsFinal_OnlyTheLastLevel()
        {
            Assert.IsTrue(_database.IsFinal(_last));
            Assert.IsFalse(_database.IsFinal(_first));
            Assert.IsFalse(_database.IsFinal(null));
        }

        [Test]
        public void ArenaOf_ReturnsTheOwningArena()
        {
            Assert.AreEqual("Arena2", _database.ArenaOf(_last).ArenaId);
        }
    }
}
