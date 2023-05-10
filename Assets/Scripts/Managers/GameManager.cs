using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

    private bool isPlayerDead = false;
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

    private void OnDisable()
    {
        PlayerController.DoorOpen -= OpenDoor;
        Lever.DoorOpen -= OpenDoor;
        PlayerController.LevelCompleted -= NextLevel;
    }


    private void OnEnable()
    {
        PlayerController.DoorOpen += OpenDoor;
        Lever.DoorOpen += OpenDoor;
        PlayerController.LevelCompleted += NextLevel;
    }

    private void Start()
    {
        _coinCount = _coinBag.transform.childCount;

        isDoorOpened = false;
        PlayerPrefs.SetInt("IsLastSceneMainMenu", 0);

        if (PlayerPrefs.HasKey("LevelCleared"))
        {

        }
        else
        {
            PlayerPrefs.SetInt("LevelCleared", 0);
            if (PlayerPrefs.HasKey("LevelClearedCount"))
            {

            }
            else
            {
                PlayerPrefs.SetInt("LevelClearedCount", 0);
            }
        }
        PlayerPrefs.SetInt("Current Level", SceneManager.GetActiveScene().buildIndex);

        if(PlayerPrefs.GetInt("Current Level") == 1)
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
        Item.quatity = 0;
        isPlayerDead = true;
        SceneManager.LoadScene("GameOver");
    }

    public void OpenDoor()
    {
        _doorOpenAnimator.enabled = true;
        isDoorOpened = true;
    }

    public void NextLevel()
    {
        int prevoiusBuildIndex = PlayerPrefs.GetInt("LevelCleared");
        int currentBuildIndex = SceneManager.GetActiveScene().buildIndex;
        int previousLevelCount = PlayerPrefs.GetInt("LevelClearedCount");


        PlayerPrefs.SetInt("CoinsCollectedQuantity", Item.quatity + PlayerPrefs.GetInt("CoinsCollectedQuantity"));

        //Total Coin quatity checking
        if(Item.quatity == _coinCount)
        {
            PlayerPrefs.SetString("CoinsCollected", "CollectedAll");
        }
        else if(Item.quatity < _coinCount && Item.quatity >= Mathf.Ceil(_coinCount / 2))
        {
            PlayerPrefs.SetString("CoinsCollected", "Collected Half");
        }
        else if(Item.quatity < _coinCount && Item.quatity >= Mathf.Ceil(_coinCount / 4))
        {
            PlayerPrefs.SetString("CoinsCollected", "Collected Quater");
        }

        if(currentBuildIndex > prevoiusBuildIndex)
        {
            PlayerPrefs.SetInt("LevelCleared", SceneManager.GetActiveScene().buildIndex);
            PlayerPrefs.SetInt("LevelClearedCount", previousLevelCount + 1);
        }

        int nextScene = SceneManager.GetActiveScene().buildIndex + 1;

        if (PlayerPrefs.GetInt("GameOverLevel") != nextScene)
        {  
            SceneManager.LoadScene("LevelSelect");
            //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
        else
        {
            int levelClearedCount = PlayerPrefs.GetInt("LevelClearedCount");
            if (PlayerPrefs.GetString("CoinsCollected") == "CollectedAll")
            {
                SaveManager.Instance.SaveJson(3, levelClearedCount - 1);

            }
            if (PlayerPrefs.GetString("CoinsCollected") == "Collected Half")
            {
                SaveManager.Instance.SaveJson(2, levelClearedCount - 1);
            }
            if (PlayerPrefs.GetString("CoinsCollected") == "Collected Quater")
            {
                SaveManager.Instance.SaveJson(1, levelClearedCount - 1);
            }
            SceneManager.LoadScene("GameCompleted");
        }


    }

    public int GetCurrentScene()
    {
        return (PlayerPrefs.GetInt("Current Level"));
    }

    private void SetPlayerBall()
    {
        for (int i = 0; i < _ballPool.BallPool.Length; i++)
        {
            if (_ballPool.BallPool[i].activeSelf)
            {
                Instantiate(_ballPool.BallPool[i].gameObject, _player.position, Quaternion.Euler(_ballPool.BallPool[i].gameObject.transform.localRotation.eulerAngles.x, _ballPool.BallPool[i].gameObject.transform.localRotation.eulerAngles.y, _ballPool.BallPool[i].gameObject.transform.localRotation.eulerAngles.z), _player);
            }
        }
    }
  
}
