using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void Retry()
    {
        SceneManager.LoadScene(GameManager.instance.GetCurrentScene());
    }

    public void QuitGame()
    {
        UIManager.Instance.QuitGame();
    }
}
