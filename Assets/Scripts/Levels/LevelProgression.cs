using Boltz.Save;

namespace Boltz.Levels
{
    /// <summary>
    /// Which levels are open to the player.
    ///
    /// A static rather than a method on the level select screen so that it can be tested against a
    /// save profile directly. The rule used to be spread across two forty line branches of
    /// LevelSelect.Start and an every frame check in Update, which is why it could disagree with
    /// itself depending on how the screen had been reached.
    /// </summary>
    public static class LevelProgression
    {
        /// <summary>
        /// A level is open once every level before it in play order has been cleared, and once its
        /// arena's star requirement has been met by the arenas ahead of it.
        /// </summary>
        public static bool IsUnlocked(LevelDatabase database, int index)
        {
            if (database == null)
                return false;

            var levels = database.All;
            if (index < 0 || index >= levels.Count)
                return false;

            for (int i = 0; i < index; i++)
            {
                if (!SaveService.IsCleared(levels[i].LevelId))
                    return false;
            }

            var arena = database.ArenaOf(levels[index]);
            if (arena == null || arena.StarsToUnlock <= 0)
                return true;

            return StarsBefore(database, arena) >= arena.StarsToUnlock;
        }

        /// <summary>Stars earned across every arena ahead of the given one.</summary>
        public static int StarsBefore(LevelDatabase database, ArenaDefinition arena)
        {
            if (database == null || arena == null)
                return 0;

            int total = 0;

            foreach (var candidate in database.Arenas)
            {
                if (candidate == null || candidate == arena)
                    break;

                foreach (var level in candidate.Levels)
                {
                    if (level != null)
                        total += SaveService.GetStars(level.LevelId);
                }
            }

            return total;
        }
    }
}
