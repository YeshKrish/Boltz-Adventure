using UnityEngine;
using Boltz.Save;

/// <summary>
/// Owns which ball skin the player has chosen.
///
/// Selection used to be stored as the active flag on the ball prefab assets themselves, which meant
/// the editor wrote runtime state into project files, two skins could be active at once, and a build
/// behaved differently from the editor because prefab assets are read only there. The choice is a
/// single index in the save profile now, and this class exists to keep that index inside the bounds
/// of the pool.
/// </summary>
public class BallManager : MonoBehaviour
{
    public static BallManager Instance;

    [SerializeField]
    private ChooseBall _ballPool;

    /// <summary>Index of the chosen skin, clamped to something the pool can actually supply.</summary>
    public int SelectedBallId => ClampToPool(SaveService.SelectedBallId);

    /// <summary>The chosen skin's prefab, or null when the pool is empty.</summary>
    public GameObject SelectedBallPrefab
    {
        get
        {
            if (_ballPool == null || _ballPool.BallPool == null || _ballPool.BallPool.Length == 0)
                return null;

            return _ballPool.BallPool[SelectedBallId];
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        DontDestroyOnLoad(this);
    }

    public void ActivateParticularBall(int ballId)
    {
        SaveService.SelectedBallId = ClampToPool(ballId);

        // Skin choice is cheap to write and annoying to lose, so it does not wait for the next
        // pause or quit.
        SaveService.Flush();
    }

    public int GetActiveball()
    {
        return SelectedBallId;
    }

    private int ClampToPool(int ballId)
    {
        if (_ballPool == null || _ballPool.BallPool == null || _ballPool.BallPool.Length == 0)
            return 0;

        return Mathf.Clamp(ballId, 0, _ballPool.BallPool.Length - 1);
    }
}
