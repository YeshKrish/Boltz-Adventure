using Boltz.UI;
using NUnit.Framework;
using UnityEngine;

namespace Boltz.Tests.EditMode
{
    /// <summary>
    /// The game is landscape only and the manifest tells Android it may lay out under a cutout,
    /// so the interesting cases are a notch taking a bite out of one of the short edges and a
    /// gesture bar taking a strip off the bottom.
    /// </summary>
    public class SafeAreaFitterTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void NoCutout_FillsTheScreen()
        {
            SafeAreaFitter.CalculateAnchors(new Rect(0, 0, 2400, 1080), 2400, 1080,
                out Vector2 min, out Vector2 max);

            Assert.AreEqual(0f, min.x, Tolerance);
            Assert.AreEqual(0f, min.y, Tolerance);
            Assert.AreEqual(1f, max.x, Tolerance);
            Assert.AreEqual(1f, max.y, Tolerance);
        }

        [Test]
        public void NotchOnTheLeft_InsetsTheLeftEdgeOnly()
        {
            // Landscape, cutout on the leading edge: exactly where the joystick lives.
            SafeAreaFitter.CalculateAnchors(new Rect(120, 0, 2280, 1080), 2400, 1080,
                out Vector2 min, out Vector2 max);

            Assert.AreEqual(0.05f, min.x, Tolerance);
            Assert.AreEqual(0f, min.y, Tolerance);
            Assert.AreEqual(1f, max.x, Tolerance);
            Assert.AreEqual(1f, max.y, Tolerance);
        }

        [Test]
        public void NotchOnTheRight_InsetsTheRightEdgeOnly()
        {
            SafeAreaFitter.CalculateAnchors(new Rect(0, 0, 2280, 1080), 2400, 1080,
                out Vector2 min, out Vector2 max);

            Assert.AreEqual(0f, min.x, Tolerance);
            Assert.AreEqual(0.95f, max.x, Tolerance);
        }

        [Test]
        public void GestureBar_InsetsTheBottom()
        {
            SafeAreaFitter.CalculateAnchors(new Rect(0, 54, 2400, 1026), 2400, 1080,
                out Vector2 min, out Vector2 max);

            Assert.AreEqual(0.05f, min.y, Tolerance);
            Assert.AreEqual(1f, max.y, Tolerance);
        }

        [Test]
        public void CutoutAndGestureBarTogether_InsetBothEdges()
        {
            SafeAreaFitter.CalculateAnchors(new Rect(120, 54, 2160, 1026), 2400, 1080,
                out Vector2 min, out Vector2 max);

            Assert.AreEqual(0.05f, min.x, Tolerance);
            Assert.AreEqual(0.05f, min.y, Tolerance);
            Assert.AreEqual(0.95f, max.x, Tolerance);
            Assert.AreEqual(1f, max.y, Tolerance);
        }

        [Test]
        public void ZeroSizedScreen_FillsTheScreenRatherThanDividingByZero()
        {
            SafeAreaFitter.CalculateAnchors(new Rect(0, 0, 0, 0), 0, 0,
                out Vector2 min, out Vector2 max);

            Assert.AreEqual(Vector2.zero, min);
            Assert.AreEqual(Vector2.one, max);
        }

        [Test]
        public void SafeAreaLargerThanTheScreen_IsClampedToTheScreen()
        {
            SafeAreaFitter.CalculateAnchors(new Rect(-40, -20, 2500, 1200), 2400, 1080,
                out Vector2 min, out Vector2 max);

            Assert.AreEqual(0f, min.x, Tolerance);
            Assert.AreEqual(0f, min.y, Tolerance);
            Assert.AreEqual(1f, max.x, Tolerance);
            Assert.AreEqual(1f, max.y, Tolerance);
        }

        [Test]
        public void RotatingTheDevice_MovesTheInsetToTheOtherEdge()
        {
            SafeAreaFitter.CalculateAnchors(new Rect(120, 0, 2280, 1080), 2400, 1080,
                out Vector2 landscapeMin, out Vector2 _);

            SafeAreaFitter.CalculateAnchors(new Rect(0, 0, 2280, 1080), 2400, 1080,
                out Vector2 flippedMin, out Vector2 flippedMax);

            Assert.AreEqual(0.05f, landscapeMin.x, Tolerance);
            Assert.AreEqual(0f, flippedMin.x, Tolerance);
            Assert.AreEqual(0.95f, flippedMax.x, Tolerance);
        }
    }
}
