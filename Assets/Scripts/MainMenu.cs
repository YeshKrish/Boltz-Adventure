using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        MusicManager.instance.ButtonClickSound();
        //MusicManager.instance.GameMusic();
        //MusicManager.instance.MainMenuMusicStop();
        SceneManager.LoadScene("LevelSelect");
    }

    public void CustomizePlayer()
    {

    }

    public void Quit()
    {
        Application.Quit();
    }

}
