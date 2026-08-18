namespace Boltz.Save
{
    /// <summary>
    /// Where a <see cref="SaveData"/> is kept. The game ships with <see cref="FileSaveStore"/>;
    /// the EditMode tests use an in-memory implementation so they never touch the disk.
    /// </summary>
    public interface ISaveStore
    {
        /// <summary>
        /// Reads the profile. Implementations must never throw: a missing, unreadable or corrupt
        /// payload returns a default <see cref="SaveData"/> instead of propagating the failure.
        /// </summary>
        /// <summary>
        /// True when a profile has previously been written. Distinguishes a genuine fresh install
        /// from a profile that failed to read, which decides whether legacy migration should run.
        /// </summary>
        bool Exists { get; }

        SaveData Load();

        void Save(SaveData data);

        void Delete();
    }
}
