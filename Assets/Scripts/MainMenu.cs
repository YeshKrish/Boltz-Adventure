using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Threading.Tasks;

public class MainMenu : MonoBehaviour
{
    public TMP_Text Coinstext;

    [SerializeField]
    private Button _muteAudio;
    [SerializeField]
    private Sprite[] _audioSprites;
    [SerializeField]
    private GameObject _comingSoon;
    [SerializeField]
    private Button _customizeButton;

    public Image _musicImage;

    private void Start()
    {
        MusicManager.instance.ChangeMainMenuMusic();

        //Is music playing check
        if (MusicManager.instance.MainMenuAudio.isPlaying)
        {
            _musicImage.sprite = _audioSprites[0];
        }
        else
        {
            _musicImage.sprite = _audioSprites[1];
        }

        PlayerPrefs.SetInt("IsLastSceneMainMenu", 1);

        int coins = 0;
        if (!PlayerPrefs.HasKey("CoinsCollectedQuantity"))
        {
            coins = 0;
            PlayerPrefs.SetInt("CoinsCollectedQuantity", 0);
           
        }
        else
        {
            coins = PlayerPrefs.GetInt("CoinsCollectedQuantity");
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
        _comingSoon.SetActive(true);
        SceneManager.LoadScene("Customize");
        //_customizeButton.interactable = false;
        //DisableCustomize();
    }

    private async void DisableCustomize()
    {
        await Task.Delay(1000);
        _comingSoon.SetActive(false);
        _customizeButton.interactable = true;
    }

    public void Quit()
    {
        Application.Quit();
    }

}
