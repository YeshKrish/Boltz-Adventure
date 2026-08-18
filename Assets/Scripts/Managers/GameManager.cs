using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Threading.Tasks;
using Boltz.Save;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [SerializeField]
    private Transform _player;
    [SerializeField]
    private Animator _doorOpenAnimator;
    [SerializeField]
    private GameObject _coinBag;
    [SerializeField]
    private ChooseBall _ballPool;

    [Header("Scripts to be deactivated")]
    [SerializeField]
    PlayerController _playerController;
    [SerializeField]
    SpecialMonsters _specialMonsters;
    //[SerializeField]
    //ProjectileMoveScript _projectileMoveScript;
    //[SerializeField]
    //EnemyController _enemyController;
    [SerializeField]
    ShootTrigger _shootTrigger;
    [SerializeField]
    Lever _lever;
    //[SerializeField]
    //SawRotate _rotate;
    [SerializeField]
    SpecialFish _specialFish;
    //[SerializeField]
    //FallingBricks _fallingBricks;

    private List<object> _scriptsToBeDeactivated;  

    private bool isPlayerDead = false;
    private bool _isPlayerKilledByEnemy = false;
    public bool isDoorOpened = false;

    private int _coinCount;

    public bool IsPlayerDead
    {
        get { return isPlayerDead; }
        set { isPlayerDead = value; }
    }
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

        SetPlayerBall();

        //Is music playing check
        if (MusicManager.instance.GameAudios[1].isPlaying)
        {
            UIManager.Instance.MusicImage.sprite = AllSceneManager.instance._audioSprites[2];
        }
        else
        {
            UIManager.Instance.MusicImage.sprite = AllSceneManager.instance._audioSprites[3];
        }
    }
    private void OnEnable()
    {
        PlayerController.KilledByEnemy += EnemyDead;
        PlayerController.DoorOpen += OpenDoor;
        Lever.DoorOpen += OpenDoor;
        PlayerController.LevelCompleted += NextLevel;
    }

    private void OnDisable()
    {
        PlayerController.KilledByEnemy -= EnemyDead;
        PlayerController.DoorOpen -= OpenDoor;
        Lever.DoorOpen -= OpenDoor;
        PlayerController.LevelCompleted -= NextLevel;
    }



    private void Start()
    {
        _isPlayerKilledByEnemy = false;
        _scriptsToBeDeactivated = new List<object>();
        _coinCount = _coinBag.transform.childCount;

        //Scripts To Be deactivated
        _scriptsToBeDeactivated.Add(_playerController);
        _scriptsToBeDeactivated.Add(_specialMonsters);
        _scriptsToBeDeactivated.Add(_specialFish);
        _scriptsToBeDeactivated.Add(_shootTrigger);
        _scriptsToBeDeactivated.Add(_lever);

        isDoorOpened = false;

        GameSession.CameFromMainMenu = false;
        GameSession.CurrentLevelBuildIndex = SceneManager.GetActiveScene().buildIndex;

        if (GameSession.CurrentLevelBuildIndex == 1)
        {
            Time.timeScale = 0;
            UIManager.Instance.JoyStick.SetActive(false);
            UIManager.Instance.JumpButton.SetActive(false);
            UIManager.Instance.PauseButton.SetActive(false);
            UIManager.Instance.Coin.SetActive(false);
        }
    }

    public void GameOver()
    {
        GameSession.CoinsThisLevel = 0;
        isPlayerDead = true;
        Destroy(_player.gameObject);
        DeactivateScripts();
        if (!_isPlayerKilledByEnemy)
        {
            SceneManager.LoadScene("GameOver");
        }
        else
        {
            GameOverPopupScreen();
        }
        
    }

    async void GameOverPopupScreen()
    {
        await Task.Delay(1000);
        SceneManager.LoadScene("GameOver");
    }

    public void OpenDoor()
    {
        _doorOpenAnimator.enabled = true;
        isDoorOpened = true;
    }

    public void NextLevel()
    {
        int coinsThisRun = GameSession.CoinsThisLevel;
        string levelId = SceneManager.GetActiveScene().name;
        int stars = StarsForCoins(coinsThisRun, _coinCount);

        SaveService.AddCoins(coinsThisRun);
        SaveService.RecordLevelResult(levelId, stars, coinsThisRun);

        // The level select screen reads these to decide which stars to pop on the way in.
        GameSession.LastRunLevelId = levelId;
        GameSession.LastRunStars = stars;

        // Finishing a level is the one moment worth writing immediately, rather than waiting for
        // the app to be paused or closed.
        SaveService.Flush();

        int nextScene = SceneManager.GetActiveScene().buildIndex + 1;

        if (nextScene != GameSession.GameCompletedBuildIndex)
        {
            SceneManager.LoadScene("LevelSelect");
        }
        else
        {
            SceneManager.LoadScene("GameCompleted");
        }
    }

    /// <summary>
    /// Stars awarded for collecting <paramref name="collected"/> of <paramref name="total"/> coins:
    /// all of them for three, half for two, a quarter for one.
    ///
    /// The halves and quarters used to be computed as Mathf.Ceil(total / 2), where the integer
    /// division happened first and made the rounding a no-op, so both thresholds could sit one coin
    /// below their intended value. Phase 6 replaces this with authored per-level thresholds.
    /// </summary>
    private static int StarsForCoins(int collected, int total)
    {
        if (total <= 0)
            return 0;

        if (collected >= total)
            return 3;

        if (collected >= Mathf.CeilToInt(total / 2f))
            return 2;

        if (collected >= Mathf.CeilToInt(total / 4f))
            return 1;

        return 0;
    }

    public int GetCurrentScene()
    {
        return GameSession.CurrentLevelBuildIndex;
    }

    private void SetPlayerBall()
    {
        if (_ballPool == null || _ballPool.BallPool == null || _ballPool.BallPool.Length == 0)
            return;

        int ballId = Mathf.Clamp(SaveService.SelectedBallId, 0, _ballPool.BallPool.Length - 1);
        var prefab = _ballPool.BallPool[ballId];
        if (prefab == null)
            return;

        // The ball prefabs are committed inactive, because selection used to be stored as their
        // active flag. Instantiating one by index therefore has to switch it on explicitly.
        var ball = Instantiate(prefab, _player.position, prefab.transform.localRotation, _player);
        ball.SetActive(true);
    }

    //W
    private void EnemyDead()
    {
        _isPlayerKilledByEnemy = true;
    }

    public void DeactivateScripts()
    {
        foreach (object scriptObject in _scriptsToBeDeactivated)
        {
            MonoBehaviour scripts = scriptObject as MonoBehaviour;
            if(scripts != null)
            {
                scripts.enabled = false;
            }
        }
    }

}
