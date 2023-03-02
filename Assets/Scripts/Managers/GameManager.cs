using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    public void GameOver()
    {
        isPlayerDead = true;
        UIManager.Instance._gameOver.SetActive(true);
        
        _player.SetActive(false);
    }

    public void OpenDoor()
    {
        _doorOpenAnimator.enabled = true;
    }

    public void NextLevel()
    {
        Debug.Log("Level 2");
    }
  
}
