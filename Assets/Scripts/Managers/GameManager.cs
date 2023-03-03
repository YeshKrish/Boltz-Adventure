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

    private void OnEnable()
    {
        PlayerController.DoorOpen += OpenDoor;
        PlayerController.LevelCompleted += NextLevel;
    }

    private void OnDisable()
    {
        PlayerController.DoorOpen -= OpenDoor;
        PlayerController.LevelCompleted -= NextLevel;
    }

    public bool IsPlayerDead
    {
        get { return isPlayerDead; }
        set { isPlayerDead = value; }
    }

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

    }

    private void Start()
    {
       
    }

    public void GameOver()
    {
        PlayerPrefs.SetInt("Current Level", SceneManager.GetActiveScene().buildIndex);
        isPlayerDead = true;
        SceneManager.LoadScene("GameOver");
    }

    public void OpenDoor()
    {
        _doorOpenAnimator.enabled = true;
    }

    public void NextLevel()
    {
        Debug.Log("Level 2");
    }

    public int GetCurrentScene()
    {
        return(PlayerPrefs.GetInt("Current Level"));
    }
  
}
