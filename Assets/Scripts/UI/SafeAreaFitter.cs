using UnityEngine;

namespace Boltz.UI
{
    /// <summary>
    /// Pins a full screen RectTransform to the part of the display that is actually usable, so the
    /// controls inside it stay clear of a notch, a punch hole camera or a gesture bar.
    ///
    /// The Android manifest has advertised notch support since the first release, which tells the
    /// system it may lay the game out underneath a cutout, and nothing in the project ever read
    /// Screen.safeArea. On a phone held with the cutout on the left, that is the edge the joystick
    /// sits on.
    ///
    /// Put this on a stretched child of the Canvas and parent the controls to it. The screen size
    /// is read every frame rather than captured once, so rotating the device, folding a foldable or
    /// resizing a window is picked up.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [DisallowMultipleComponent]
    public class SafeAreaFitter : MonoBehaviour
    {
        private RectTransform _rect;
        private Rect _appliedSafeArea;
        private int _appliedScreenWidth;
        private int _appliedScreenHeight;

        /// <summary>
        /// Turns a safe area in pixels into the anchors a full screen RectTransform needs.
        ///
        /// Separated from the component so the arithmetic can be tested against real device
        /// geometry without a device. A screen with no dimensions, which happens on Android when
        /// the app is being sent to the background, produces the full screen anchors rather than a
        /// division by zero.
        /// </summary>
        public static void CalculateAnchors(Rect safeArea, int screenWidth, int screenHeight,
            out Vector2 anchorMin, out Vector2 anchorMax)
        {
            if (screenWidth <= 0 || screenHeight <= 0)
            {
                anchorMin = Vector2.zero;
                anchorMax = Vector2.one;
                return;
            }

            anchorMin = new Vector2(
                Mathf.Clamp01(safeArea.xMin / screenWidth),
                Mathf.Clamp01(safeArea.yMin / screenHeight));

            anchorMax = new Vector2(
                Mathf.Clamp01(safeArea.xMax / screenWidth),
                Mathf.Clamp01(safeArea.yMax / screenHeight));
        }

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();
        }

        private void OnEnable()
        {
            Apply();
        }

        private void Update()
        {
            if (Screen.safeArea == _appliedSafeArea
                && Screen.width == _appliedScreenWidth
                && Screen.height == _appliedScreenHeight)
            {
                return;
            }

            Apply();
        }

        private void Apply()
        {
            Rect safeArea = Screen.safeArea;

            _appliedSafeArea = safeArea;
            _appliedScreenWidth = Screen.width;
            _appliedScreenHeight = Screen.height;

            CalculateAnchors(safeArea, Screen.width, Screen.height, out Vector2 anchorMin, out Vector2 anchorMax);

            _rect.anchorMin = anchorMin;
            _rect.anchorMax = anchorMax;
            _rect.offsetMin = Vector2.zero;
            _rect.offsetMax = Vector2.zero;
        }
    }
}
