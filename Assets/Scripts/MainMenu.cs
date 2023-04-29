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
    private GameObject _comingSoon;
    [SerializeField]
    private Button _customizeButton;
    [SerializeField]
    private Animator _logoAnimation;
    [SerializeField]
    private Animator _boardAnimation;
    [SerializeField]
    private GameObject _musicButton;

    public Image _musicImage;

    private bool _isSettingsActivated = false;

    private void Start()
    {
        MusicManager.instance.ChangeMainMenuMusic();
        PlayAnimation();

        //Is music playing check
        if (MusicManager.instance.GameAudios[0].isPlaying)
        {
            _musicImage.sprite = AllSceneManager.instance._audioSprites[0];
        }
        else
        {
            _musicImage.sprite = AllSceneManager.instance._audioSprites[1];
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
        if (MusicManager.instance._isGameAudioMuted)
        {
            _musicImage.sprite = AllSceneManager.instance._audioSprites[0];
            MusicManager.instance.MuteOrUmuteGameAudio();
        }
        else
        {
            _musicImage.sprite = AllSceneManager.instance._audioSprites[1];
            MusicManager.instance.MuteOrUmuteGameAudio();
        }
    }

    public void StartGame()
    {
        MusicManager.instance.ButtonClickSound();
        MainMenuChangedOnce();
        SceneManager.LoadScene("LevelSelect");
    }

    public void CustomizePlayer()
    {
        SceneManager.LoadScene("Customize");
        MainMenuChangedOnce();
    }

    public void PopUpSettings()
    {
        if (!_isSettingsActivated)
        {
            _isSettingsActivated = true;
            StartCoroutine(PopUP());
        }
        else if(_isSettingsActivated){
            _isSettingsActivated = false;
            StartCoroutine(PopDown());
        }
    }

    IEnumerator PopUP()
    {
        yield return new WaitForSeconds(.3f);
        _musicButton.SetActive(true);
    }   
    IEnumerator PopDown()
    {
        yield return new WaitForSeconds(.3f);
        _musicButton.SetActive(false);
    }

    public void Quit()
    {
        Application.Quit();
    }

    private void MainMenuChangedOnce()
    {
        PlayerPrefs.SetInt("IsMainMenuChnagedAtLeastOnce", 1);
    }

    private void PlayAnimation()
    {
        if(PlayerPrefs.GetInt("IsMainMenuChnagedAtLeastOnce") == 0)
        {
            _logoAnimation.enabled = true;
            _boardAnimation.enabled = true;
        }
    }
}
