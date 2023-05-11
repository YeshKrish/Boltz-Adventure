using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    public void MainMenu()
    {
        MusicManager.instance.ButtonClickSound();
        SceneManager.LoadScene("MainMenu");
    }

    public void Retry()
    {
        MusicManager.instance.ButtonClickSound();
        SceneManager.LoadScene(GameManager.instance.GetCurrentScene());
    }

    public void QuitGame()
    {
        MusicManager.instance.ButtonClickSound();
        Application.Quit();
    }
}
