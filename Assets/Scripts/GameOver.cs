using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    public void MainMenu()
    {
        MusicManager.Instance.ButtonClickSound();
        SceneManager.LoadScene("MainMenu");
    }

    public void Retry()
    {
        MusicManager.Instance.ButtonClickSound();
        SceneManager.LoadScene(GameManager.Instance.GetCurrentScene());
    }

    public void QuitGame()
    {
        MusicManager.Instance.ButtonClickSound();
        Application.Quit();
    }
}
