using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public GameObject _gameOver;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GameStart()
    {
        Time.timeScale = 1f;
    }

    public void RetryLevel()
    {
        MusicManager.instance.ButtonClickSound();
        SceneManager.LoadScene(GameManager.instance.GetCurrentScene());
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
