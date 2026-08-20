using System;
using System.Collections;
using Boltz.Levels;
using UnityEngine;
using UnityEngine.UI;

namespace Boltz.UI
{
    /// <summary>
    /// One level's tile on the level select screen.
    ///
    /// Replaces chains like GetChild(0).GetChild(2).GetChild(1), which meant the screen's behaviour
    /// depended on the sibling order inside a prefab. Reordering two children in the inspector used
    /// to silently swap the lock for a star.
    /// </summary>
    public class LevelButtonView : MonoBehaviour
    {
        [SerializeField]
        private Button _button;

        [SerializeField]
        private GameObject _lock;

        [Tooltip("Plays the lock opening. Kept disabled until the tile is actually being unlocked.")]
        [SerializeField]
        private Animator _lockAnimator;

        [Tooltip("Three star images, dimmest first.")]
        [SerializeField]
        private GameObject[] _stars;

        /// <summary>How long the lock opening animation runs before the tile becomes playable.</summary>
        private const float UnlockAnimationSeconds = 1f;

        /// <summary>The level this tile stands for, or null for a slot with no level behind it.</summary>
        public LevelDefinition Level { get; private set; }

        public bool IsUnlocked { get; private set; }

        /// <summary>Raised when an unlocked tile is tapped. Slots with no level never raise it.</summary>
        public event Action<LevelDefinition> Clicked;

        private void Awake()
        {
            if (_button != null)
                _button.onClick.AddListener(OnClicked);
        }

        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveListener(OnClicked);
        }

        /// <summary>
        /// Points the tile at a level and shows its current state.
        ///
        /// A null <paramref name="level"/> is a slot the level select screen still lays out but has
        /// no content for, such as the four unbuilt places in arena 2. Those stay locked and
        /// unplayable rather than being wired to whatever scene happens to sit at that build index.
        /// </summary>
        public void Bind(LevelDefinition level, int stars, bool unlocked)
        {
            Level = level;
            IsUnlocked = level != null && unlocked;

            if (_button != null)
                _button.interactable = IsUnlocked;

            if (_lockAnimator != null)
                _lockAnimator.enabled = false;

            if (_lock != null)
                _lock.SetActive(!IsUnlocked);

            ShowStars(IsUnlocked ? stars : 0);
        }

        /// <summary>Lights the first <paramref name="stars"/> of the three star images.</summary>
        public void ShowStars(int stars)
        {
            if (_stars == null)
                return;

            for (int i = 0; i < _stars.Length; i++)
            {
                if (_stars[i] != null)
                    _stars[i].SetActive(i < stars);
            }
        }

        /// <summary>
        /// Plays the lock opening, then leaves the tile playable.
        ///
        /// A coroutine rather than the async void plus Task.Delay this replaced, so that leaving the
        /// screen mid animation stops it instead of resuming against a destroyed tile.
        /// </summary>
        public IEnumerator PlayUnlock()
        {
            if (_lockAnimator != null)
                _lockAnimator.enabled = true;

            yield return new WaitForSeconds(UnlockAnimationSeconds);

            IsUnlocked = true;

            if (_lock != null)
                _lock.SetActive(false);

            if (_button != null)
                _button.interactable = true;
        }

        private void OnClicked()
        {
            if (Level == null)
                return;

            var handler = Clicked;
            if (handler != null)
                handler(Level);
        }

        private void Reset()
        {
            _button = GetComponent<Button>();
        }
    }
}
