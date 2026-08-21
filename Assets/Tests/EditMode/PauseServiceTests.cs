using NUnit.Framework;
using UnityEngine;

namespace Boltz.Tests.EditMode
{
    /// <summary>
    /// The clock used to have eight owners and the pause menu could disagree with itself about
    /// whether the game was paused. These cover the part that decides, so a future caller adding
    /// a third reason to pause cannot quietly reintroduce the case where dismissing one screen
    /// starts the clock while another screen still wants it stopped.
    /// </summary>
    public class PauseServiceTests
    {
        [SetUp]
        [TearDown]
        public void ClearHolds()
        {
            PauseService.ReleaseAll();
            Time.timeScale = 1f;
        }

        [Test]
        public void NothingHeld_ClockRuns()
        {
            Assert.IsFalse(PauseService.IsPaused);
            Assert.AreEqual(1f, Time.timeScale);
        }

        [Test]
        public void Hold_StopsTheClock()
        {
            PauseService.Hold(PauseReason.PauseScreen);

            Assert.IsTrue(PauseService.IsPaused);
            Assert.IsTrue(PauseService.IsHeldBy(PauseReason.PauseScreen));
            Assert.AreEqual(0f, Time.timeScale);
        }

        [Test]
        public void ReleasingTheOnlyHold_StartsTheClock()
        {
            PauseService.Hold(PauseReason.PauseScreen);
            PauseService.Release(PauseReason.PauseScreen);

            Assert.IsFalse(PauseService.IsPaused);
            Assert.AreEqual(1f, Time.timeScale);
        }

        [Test]
        public void ReleasingOneOfTwoHolds_LeavesTheClockStopped()
        {
            PauseService.Hold(PauseReason.Instructions);
            PauseService.Hold(PauseReason.PauseScreen);

            PauseService.Release(PauseReason.PauseScreen);

            Assert.IsTrue(PauseService.IsPaused, "the instruction screen still wants the clock stopped");
            Assert.IsFalse(PauseService.IsHeldBy(PauseReason.PauseScreen));
            Assert.AreEqual(0f, Time.timeScale);
        }

        [Test]
        public void ReleasingBothHolds_StartsTheClock()
        {
            PauseService.Hold(PauseReason.Instructions);
            PauseService.Hold(PauseReason.PauseScreen);

            PauseService.Release(PauseReason.PauseScreen);
            PauseService.Release(PauseReason.Instructions);

            Assert.IsFalse(PauseService.IsPaused);
            Assert.AreEqual(1f, Time.timeScale);
        }

        [Test]
        public void HoldingTwice_IsUndoneByOneRelease()
        {
            PauseService.Hold(PauseReason.PauseScreen);
            PauseService.Hold(PauseReason.PauseScreen);

            PauseService.Release(PauseReason.PauseScreen);

            Assert.IsFalse(PauseService.IsPaused, "a reason holds once however often it asks");
            Assert.AreEqual(1f, Time.timeScale);
        }

        [Test]
        public void ReleasingSomethingThatWasNotHolding_DoesNothing()
        {
            PauseService.Hold(PauseReason.Instructions);

            PauseService.Release(PauseReason.PauseScreen);

            Assert.IsTrue(PauseService.IsPaused);
            Assert.AreEqual(0f, Time.timeScale);
        }

        [Test]
        public void ReleaseAll_DropsEveryHold()
        {
            PauseService.Hold(PauseReason.Instructions);
            PauseService.Hold(PauseReason.PauseScreen);

            PauseService.ReleaseAll();

            Assert.IsFalse(PauseService.IsPaused);
            Assert.IsFalse(PauseService.IsHeldBy(PauseReason.Instructions));
            Assert.IsFalse(PauseService.IsHeldBy(PauseReason.PauseScreen));
            Assert.AreEqual(1f, Time.timeScale);
        }
    }
}
