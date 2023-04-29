using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public GameObject QuestTextObj;

    [SerializeField]
    private TMP_Text _coinText; 
    [SerializeField]
    private GameObject _pauseScreen;

    public Image MusicImage;

    private bool _isGamePaused = false;

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
            UIManager.Instance.MusicImage.sprite = AllSceneManager.instance._audioSprites[0];
            MusicManager.instance.MuteOrUmuteGameAudio();
        }
        else
        {
            UIManager.Instance.MusicImage.sprite = AllSceneManager.instance._audioSprites[1];
            MusicManager.instance.MuteOrUmuteGameAudio();
        }
    }
}
