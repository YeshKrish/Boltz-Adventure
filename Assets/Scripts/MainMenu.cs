using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{
    public TMP_Text Coinstext;

    private void Start()
    {
        PlayerPrefs.SetInt("IsLastSceneMainMenu", 1);

        int coins = 0;
        if (!PlayerPrefs.HasKey("CoinsCollected"))
        {
            coins = 0;
            PlayerPrefs.SetInt("CoinsCollected", 0);
           
        }
        else
        {
            coins = PlayerPrefs.GetInt("CoinsCollected");
        }

        Coinstext.SetText(coins.ToString());
    }

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
