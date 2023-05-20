using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

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

    private bool _isGamePaused = false;
    private int _presentInstruction = 0;


    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void Start()
    {
        Item.quatity = 0;
        _coinText.text = Item.quatity.ToString();
    }

    public void UpdateScoreText()
    {
        Item.quatity = Item.quatity + 1;
        if (Item.quatity < 10)
        {
            _coinText.text = "0" + Item.quatity.ToString();
        }
        else
        {
            _coinText.text = Item.quatity.ToString();
        }
    }

    public void GameStart()
    {
        Time.timeScale = 1f;
    }

    public void RetryLevel()
    {
        MusicManager.instance.ButtonClickSound();
        SceneManager.LoadScene(GameManager.instance.GetCurrentScene());
        Time.timeScale = 1f;
    }

    public void QuitGame()
    {
        MusicManager.instance.ButtonClickSound();
        Application.Quit();
    }

    public void MainMenu()
    {
        Time.timeScale = 1;
        MusicManager.instance.ButtonClickSound();
        SceneManager.LoadScene("MainMenu");
    }

    public void PauseScreen()
    {
        MusicManager.instance.ButtonClickSound();
        if (!_isGamePaused)
        {
            _isGamePaused = true;
            Time.timeScale = 0;
            _pauseScreen.SetActive(true);
        }
        else
        {
            Time.timeScale = 1;
            _isGamePaused &= false;
            _pauseScreen.SetActive(false);
        }
    }
    public void ResumeGame()
    {
        MusicManager.instance.ButtonClickSound();
        Time.timeScale = 1;
        _pauseScreen.SetActive(false);
    }
    public void MuteAudio()
    {
        if (MusicManager.instance._isGameAudioMuted)
        {
            MusicImage.sprite = AllSceneManager.instance._audioSprites[2];
            MusicManager.instance.MuteOrUmuteGameAudio();
        }
        else
        {
            MusicImage.sprite = AllSceneManager.instance._audioSprites[3];
            MusicManager.instance.MuteOrUmuteGameAudio();
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
            Time.timeScale = 1;
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

}
