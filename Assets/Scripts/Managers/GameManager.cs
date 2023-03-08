using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [SerializeField]
    private GameObject _player;
    [SerializeField]
    private Animator _doorOpenAnimator;

    private bool isPlayerDead = false;
    public bool isDoorOpened = false;

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

        PlayerPrefs.SetInt("CoinsCollected", Item.quatity + PlayerPrefs.GetInt("CoinsCollected"));

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
            SceneManager.LoadScene("GameCompleted");
        }
    }

    public int GetCurrentScene()
    {
        return(PlayerPrefs.GetInt("Current Level"));
    }
  
}
