using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Threading.Tasks;
using Boltz.Save;


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

    [SerializeField]
    private Image _musicImage;

    private bool _isSettingsActivated = false;
    private float _originalAlpha;
    private Tween _musicImageFade;

    private void Start()
    {
        MusicManager.Instance.ChangeMainMenuMusic();
        PlayAnimation();
        _originalAlpha = _musicButton.GetComponent<Image>().color.a;

        //Is music playing check
        if (MusicManager.Instance.GameAudios[0].isPlaying)
        {
            _musicImage.sprite = AllSceneManager.Instance.AudioSprites[0];
        }
        else
        {
            _musicImage.sprite = AllSceneManager.Instance.AudioSprites[1];
        }

        GameSession.CameFromMainMenu = true;

        Coinstext.SetText(SaveService.TotalCoins.ToString());

    }

    private void OnDisable()
    {
        _musicImageFade?.Kill();
        _musicImageFade = null;
    }

    public void MuteAudio()
    {
        if (MusicManager.Instance.IsGameAudioMuted)
        {
            _musicImage.sprite = AllSceneManager.Instance.AudioSprites[0];
            MusicManager.Instance.MuteOrUmuteGameAudio();
        }
        else
        {
            _musicImage.sprite = AllSceneManager.Instance.AudioSprites[1];
            MusicManager.Instance.MuteOrUmuteGameAudio();
        }
    }

    public void StartGame()
    {
        MusicManager.Instance.ButtonClickSound();
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
            _musicImageFade?.Kill();
            _musicImageFade = _musicImage.DOFade(1f, 0.1f)
                .SetEase(Ease.Linear)
                .SetLink(gameObject);
            StartCoroutine(PopUP());
        }
        else if(_isSettingsActivated){
            _isSettingsActivated = false;
            _musicButton.GetComponent<Button>().interactable = false;
            _musicImageFade?.Kill();
            _musicImageFade = _musicImage.DOFade(0f, 0.4f)
                .SetEase(Ease.Linear)
                .SetLink(gameObject)
                .OnComplete(PopDown);
        }
    }

    IEnumerator PopUP()
    {
        yield return new WaitForSeconds(.3f);
        _musicButton.SetActive(true);
    }   
    private void PopDown()
    {
        _musicButton.SetActive(false);
        _musicButton.GetComponent<Button>().interactable = true;
    }

    public void Quit()
    {
        Application.Quit();
    }

    private void MainMenuChangedOnce()
    {
        GameSession.HasLeftMainMenu = true;
    }

    private void PlayAnimation()
    {
        if (!GameSession.HasLeftMainMenu)
        {
            _logoAnimation.enabled = true;
            _boardAnimation.enabled = true;
        }
    }
}
