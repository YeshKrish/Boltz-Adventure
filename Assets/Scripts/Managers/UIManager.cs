using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using Boltz.Save;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public GameObject QuestTextObj;
    public GameObject FishTextObj;

    [SerializeField]
    private TMP_Text _coinText; 
    [SerializeField]
    private GameObject _pauseScreen;
    [SerializeField]
    private GameObject _instructionScreen;
    [SerializeField]
    private List<Sprite> _instructionImages;
    public GameObject JoyStick;    
    public GameObject JumpButton;
    public GameObject PauseButton;
    public GameObject Coin;

    public Image MusicImage;

    private int _presentInstruction = 0;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        GameSession.CoinsThisLevel = 0;
        _coinText.text = GameSession.CoinsThisLevel.ToString();
    }

    public void UpdateScoreText()
    {
        GameSession.CoinsThisLevel++;
        if (GameSession.CoinsThisLevel < 10)
        {
            _coinText.text = "0" + GameSession.CoinsThisLevel.ToString();
        }
        else
        {
            _coinText.text = GameSession.CoinsThisLevel.ToString();
        }
    }

    public void RetryLevel()
    {
        MusicManager.Instance.ButtonClickSound();
        PauseService.ReleaseAll();
        SceneManager.LoadScene(GameManager.Instance.GetCurrentScene());
    }

    public void QuitGame()
    {
        MusicManager.Instance.ButtonClickSound();
        Application.Quit();
    }

    public void MainMenu()
    {
        MusicManager.Instance.ButtonClickSound();
        PauseService.ReleaseAll();
        SceneManager.LoadScene("MainMenu");
    }

    public void PauseScreen()
    {
        MusicManager.Instance.ButtonClickSound();

        if (PauseService.IsHeldBy(PauseReason.PauseScreen))
        {
            ClosePauseScreen();
        }
        else
        {
            PauseService.Hold(PauseReason.PauseScreen);
            _pauseScreen.SetActive(true);
        }
    }

    public void ResumeGame()
    {
        MusicManager.Instance.ButtonClickSound();
        ClosePauseScreen();
    }

    private void ClosePauseScreen()
    {
        PauseService.Release(PauseReason.PauseScreen);
        _pauseScreen.SetActive(false);
    }
    public void MuteAudio()
    {
        if (MusicManager.Instance.IsGameAudioMuted)
        {
            MusicImage.sprite = AllSceneManager.Instance.AudioSprites[2];
            MusicManager.Instance.MuteOrUmuteGameAudio();
        }
        else
        {
            MusicImage.sprite = AllSceneManager.Instance.AudioSprites[3];
            MusicManager.Instance.MuteOrUmuteGameAudio();
        }
    }

    public void Close()
    {
        if (_instructionScreen.activeSelf)
        {
            _instructionScreen.SetActive(false);
            JoyStick.SetActive(true);
            JumpButton.SetActive(true);
            PauseButton.SetActive(true);
            Coin.SetActive(true);
            PauseService.Release(PauseReason.Instructions);
        }
    }

    public void NextImage()
    {
        _presentInstruction = _presentInstruction + 1;
        if(_presentInstruction == _instructionImages.Count)
        {
            _presentInstruction = 0;
        }
        _instructionScreen.GetComponent<Image>().sprite = _instructionImages[_presentInstruction];
    }    
    public void PreviousImage()
    {
        _presentInstruction = _presentInstruction - 1;
        if(_presentInstruction < 0)
        {
            _presentInstruction = _instructionImages.Count - 1;
        }
        _instructionScreen.GetComponent<Image>().sprite = _instructionImages[_presentInstruction];
    }

    public void HideUI()
    {
        JoyStick.SetActive(false);
        JumpButton.SetActive(false);
        PauseButton.SetActive(false);
        Coin.SetActive(false);
    }
    public void ActivateUI()
    {
        JoyStick.SetActive(true);
        JumpButton.SetActive(true);
        PauseButton.SetActive(true);
        Coin.SetActive(true);
    }
}
