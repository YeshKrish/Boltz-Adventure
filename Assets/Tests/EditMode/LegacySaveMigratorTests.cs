using Boltz.Save;
using NUnit.Framework;

namespace Boltz.Tests.EditMode
{
    /// <summary>
    /// The version 1.1 star file was written with File.AppendAllText, so real devices hold files
    /// with repeated keys, partial lines from interrupted writes, and trailing blanks. A player
    /// upgrading from the store build must keep their progress, so none of that may throw.
    /// </summary>
    public class LegacySaveMigratorTests
    {
        // Stands in for the build settings: ordinal 0 is the first level.
        private static string LevelIdForOrdinal(int ordinal)
        {
            return ordinal >= 0 && ordinal < 6 ? "Level" + (ordinal + 1) : null;
        }

        private static SaveData Apply(params string[] lines)
        {
            var data = new SaveData();
            LegacySaveMigrator.ApplyStarLines(data, lines, LevelIdForOrdinal);
            return data;
        }

        [Test]
        public void ReadsATabSeparatedLine()
        {
            var data = Apply("0\t3");

            Assert.AreEqual(3, data.FindLevel("Level1").Stars);
            Assert.IsTrue(data.FindLevel("Level1").Cleared);
        }

        [Test]
        public void TakesTheBestValueWhenAnOrdinalRepeats()
        {
            // What an appending writer actually produces after replaying a level.
            var data = Apply("0\t1", "0\t3", "0\t2");

            Assert.AreEqual(3, data.FindLevel("Level1").Stars);
        }

        [Test]
        public void SkipsMalformedLinesAndKeepsTheRest()
        {
            var data = Apply("0\t3", "garbage", "1", "\t", "2\tnotanumber", "3\t2");

            Assert.AreEqual(3, data.FindLevel("Level1").Stars);
            Assert.AreEqual(2, data.FindLevel("Level4").Stars);
            Assert.AreEqual(2, data.Levels.Count, "Only the two readable lines should have produced records.");
        }

        [Test]
        public void SkipsBlankAndEmptyLines()
        {
            var data = Apply(string.Empty, "   ", "0\t2", null);

            Assert.AreEqual(1, data.Levels.Count);
            Assert.AreEqual(2, data.FindLevel("Level1").Stars);
        }

        [Test]
        public void IgnoresOrdinalsThatDoNotMapToALevel()
        {
            var data = Apply("99\t3", "-1\t3", "0\t1");

            Assert.AreEqual(1, data.Levels.Count);
            Assert.AreEqual(1, data.FindLevel("Level1").Stars);
        }

        [Test]
        public void ClampsStarsIntoRange()
        {
            var data = Apply("0\t99", "1\t-5");

            Assert.AreEqual(3, data.FindLevel("Level1").Stars);
            Assert.AreEqual(0, data.FindLevel("Level2").Stars);
        }

        [Test]
        public void DoesNotLowerAResultAlreadyPresent()
        {
            var data = new SaveData();
            data.GetOrCreateLevel("Level1").Stars = 3;

            LegacySaveMigrator.ApplyStarLines(data, new[] { "0\t1" }, LevelIdForOrdinal);

            Assert.AreEqual(3, data.FindLevel("Level1").Stars);
        }

        [Test]
        public void ReportsWhetherAnythingWasImported()
        {
            var data = new SaveData();

            Assert.IsFalse(LegacySaveMigrator.ApplyStarLines(data, new[] { "junk" }, LevelIdForOrdinal));
            Assert.IsTrue(LegacySaveMigrator.ApplyStarLines(data, new[] { "0\t2" }, LevelIdForOrdinal));
        }

        [Test]
        public void ToleratesNullArguments()
        {
            Assert.IsFalse(LegacySaveMigrator.ApplyStarLines(null, new[] { "0\t1" }, LevelIdForOrdinal));
            Assert.IsFalse(LegacySaveMigrator.ApplyStarLines(new SaveData(), null, LevelIdForOrdinal));
            Assert.IsFalse(LegacySaveMigrator.ApplyStarLines(new SaveData(), new[] { "0\t1" }, null));
        }
    }
}
