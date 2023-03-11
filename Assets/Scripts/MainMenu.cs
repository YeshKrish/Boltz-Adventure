using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{
    public TMP_Text Coinstext;

    [SerializeField]
    private Button _muteAudio;
    [SerializeField]
    private Sprite[] _audioSprites;

    public Image _musicImage;

    private void Start()
    {
        MusicManager.instance.ChangeMainMenuMusic();

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

    public void MuteAudio()
    {
        Debug.Log(MusicManager.instance.MainMenuAudio.isPlaying);
        if (!MusicManager.instance.MainMenuAudio.isPlaying)
        {
            _musicImage.sprite = _audioSprites[0];
            MusicManager.instance.MainMenuAudio.Play();
        }
        else
        {
            _musicImage.sprite = _audioSprites[1];
            MusicManager.instance.MainMenuAudio.Stop();
        }
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
