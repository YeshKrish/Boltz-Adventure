using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [SerializeField]
    private GameObject _player;

    private int _enemyDanceAnimationId;

    private bool isPlayerDead = false;

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
  
}
