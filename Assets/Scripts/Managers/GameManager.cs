using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Threading.Tasks;
using Boltz.Levels;
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

        var level = LevelFlow.Current;

        // The star thresholds are authored, so they go stale the moment somebody adds or removes a
        // coin in the scene. Cheap to check, and silent drift here quietly changes what three stars
        // means.
        if (level != null && level.TotalCoins != _coinCount)
        {
            Debug.LogWarning(
                level.name + " is authored with " + level.TotalCoins + " coins but the scene contains "
                + _coinCount + ".", this);
        }

        if (level != null && level.ShowsIntro)
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
        bool wasFinalLevel = LevelFlow.CompleteCurrent(GameSession.CoinsThisLevel);

        SceneManager.LoadScene(wasFinalLevel ? "GameCompleted" : "LevelSelect");
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
