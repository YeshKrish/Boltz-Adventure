using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelect : MonoBehaviour
{
    [SerializeField]
    private Button[] _levelsToUnlock;
    [SerializeField]
    private GameObject[] _locksToUnlock;
    [SerializeField]
    private GameObject[] _stars;
    [SerializeField]
    private List<GameObject> _arena;
    [SerializeField]
    private List<Button> _nextAndPreviousArenaButtons;
    [SerializeField]
    private Animator _ownDisappearingAnimation;
    [SerializeField]
    private GameObject _owl;

    private bool _isArenaCompleted = false;
    private bool _isOwlDisappered = false;

    //Animation
    private string _isNewArenaUnlockedHash;

    private static LevelSelect instance;
    private List<int> _levelsCompleted;

    private static List<int> _previousLevelClearedCount = new List<int>();

    private Dictionary<int, int> _levelCompleteAndStarsGainedDict = new Dictionary<int, int>();

    private int _presentArena = 0;

    //total Arena stars
    private int _totalArenaStars = 80;
    private int totalStars = 0;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void Start()
    {
        _isNewArenaUnlockedHash = "_isNewArenaUnlocked";
        _nextAndPreviousArenaButtons[0].interactable = false;
        LoadDictionary();

        DisableAll();
        if (!PlayerPrefs.HasKey("LevelClearedCount"))
            PlayerPrefs.SetInt("LevelClearedCount", 0);

        int levelClearedCount = PlayerPrefs.GetInt("LevelClearedCount");
        FindIfArenaCompleted(levelClearedCount);

        //Checking if it is a new level, if nw adding the levelCleareddCount to previouseLevelCount list
        if (levelClearedCount > 0 && !_previousLevelClearedCount.Contains(levelClearedCount))
        {
            //If LevelSelect screen loads from a Level
            if (PlayerPrefs.GetInt("IsLastSceneMainMenu") == 0)
            {
                int startsColected = 0;
                //No of stars to be poped up
                if (PlayerPrefs.GetString("CoinsCollected") == "CollectedAll")
                {
                    startsColected = 3;
                    StarPopper(levelClearedCount - 1, 3);
                }
                if (PlayerPrefs.GetString("CoinsCollected") == "Collected Half")
                {
                    startsColected = 2;
                    StarPopper(levelClearedCount - 1, 2);
                }
                if (PlayerPrefs.GetString("CoinsCollected") == "Collected Quater")
                {
                    startsColected = 1;
                    StarPopper(levelClearedCount - 1, 1);
                }
                SaveManager.Instance.SaveJson(startsColected, levelClearedCount - 1);
                LoadDictionary();

                for (int i = 0; i < levelClearedCount; i++)
                {
                    Debug.Log("I" + i);
                    foreach (KeyValuePair<int, int> keyValuePair in _levelCompleteAndStarsGainedDict)
                    {
                        StarPopper(keyValuePair.Key, keyValuePair.Value);
                    }
                    _levelsToUnlock[i].interactable = true;
                    _levelsToUnlock[i].transform.GetChild(0).GetChild(1).gameObject.SetActive(false);
                }

                if(levelClearedCount % 5 != 0)
                {
                    _levelsToUnlock[levelClearedCount].transform.GetChild(0).GetChild(1).gameObject.GetComponent<Animator>().enabled = true;
                    StartCoroutine(DisableLockWithAnimation(levelClearedCount));
                }
                else if(levelClearedCount % 5 == 0)
                {
                    _arena[0].SetActive(false);
                    _arena[1].SetActive(true);
                    if (totalStars == _totalArenaStars)
                    {
                        _isOwlDisappered = true;
                        _arena[0].SetActive(false);
                        _arena[1].SetActive(true);
                        ArenaCompletionAnimationAndUnlockLogic(levelClearedCount);
                    }
                }
            }
            //If LevelSelect screen loads from a Menu
            else
            {
                if (levelClearedCount == 6)
                {
                    foreach (KeyValuePair<int, int> keyValuePair in _levelCompleteAndStarsGainedDict)
                    {
                        StarPopper(keyValuePair.Key, keyValuePair.Value);
                    }
                    for (int i = 0; i < levelClearedCount; i++)
                    {
                        _levelsToUnlock[i].interactable = true;
                        _levelsToUnlock[i].transform.GetChild(0).GetChild(1).gameObject.SetActive(false);
                    }
                }
                else
                {
                    foreach (KeyValuePair<int, int> keyValuePair in _levelCompleteAndStarsGainedDict)
                    {
                        StarPopper(keyValuePair.Key, keyValuePair.Value);
                    }
                    for (int i = 0; i < levelClearedCount + 1; i++)
                    {
                        _levelsToUnlock[i].interactable = true;
                        _levelsToUnlock[i].transform.GetChild(0).GetChild(1).gameObject.SetActive(false);
                    }
                }
            }
            _previousLevelClearedCount.Add(levelClearedCount);
        }
        else if (levelClearedCount > 0 && _previousLevelClearedCount.Contains(levelClearedCount))
        {
            int startsColected = 0;
            int currentLevel = PlayerPrefs.GetInt("Current Level");

            if (PlayerPrefs.GetString("CoinsCollected") == "CollectedAll")
            {
                startsColected = 3;
            }
            if (PlayerPrefs.GetString("CoinsCollected") == "Collected Half")
            {
                startsColected = 2;
            }
            if (PlayerPrefs.GetString("CoinsCollected") == "Collected Quater")
            {
                startsColected = 1;
            }
            if (startsColected > 0 && PlayerPrefs.GetInt("IsLastSceneMainMenu") == 0)
            {
                SaveManager.Instance.OverrideJson(currentLevel - 1, startsColected);
                LoadDictionary();
            }

            if (levelClearedCount == 6)
            {
                foreach (KeyValuePair<int, int> keyValuePair in _levelCompleteAndStarsGainedDict)
                {
                    StarPopper(keyValuePair.Key, keyValuePair.Value);
                }
                for (int i = 0; i < levelClearedCount; i++)
                {
                    _levelsToUnlock[i].interactable = true;
                    _levelsToUnlock[i].transform.GetChild(0).GetChild(1).gameObject.SetActive(false);
                }
            }
            else
            {
                foreach (KeyValuePair<int, int> keyValuePair in _levelCompleteAndStarsGainedDict)
                {
                    StarPopper(keyValuePair.Key, keyValuePair.Value);
                }
                for (int i = 0; i < levelClearedCount + 1; i++)
                {
                    _levelsToUnlock[i].interactable = true;
                    _levelsToUnlock[i].transform.GetChild(0).GetChild(1).gameObject.SetActive(false);
                }
            }
        }
        else if (levelClearedCount == 0)
        {
            _levelsToUnlock[levelClearedCount].interactable = true;
            _levelsToUnlock[levelClearedCount].transform.GetChild(0).GetChild(1).gameObject.SetActive(false);
        }
    }

    async void ArenaCompletionAnimationAndUnlockLogic(int levelClearedCount)
    {
        await Task.Delay(300);
        _ownDisappearingAnimation.SetBool(_isNewArenaUnlockedHash, true);
        _levelsToUnlock[levelClearedCount].transform.GetChild(0).GetChild(1).gameObject.GetComponent<Animator>().enabled = true;
        StartCoroutine(DisableLockWithAnimation(levelClearedCount));
    }

    IEnumerator DisableLockWithAnimation(int lockNo)
    {
        yield return new WaitForSeconds(1f);
        _levelsToUnlock[lockNo].transform.GetChild(0).GetChild(1).gameObject.SetActive(false);
        _levelsToUnlock[lockNo].interactable = true;
    }


    public void LevelToBeOpened(int level)
    {
        MusicManager.instance.ButtonClickSound();
        MusicManager.instance.GameMusic();
        MusicManager.instance.MainMenuMusicStop();
        SceneManager.LoadScene(level);
    }

    private void DisableAll()
    {
        foreach (var levels in _levelsToUnlock)
        {
            levels.interactable = false;
        }
        foreach (GameObject animators in _locksToUnlock)
        {
            animators.GetComponent<Animator>().enabled = false;
        }
    }
    public void MainMenu()
    {
        NavigationManager.Instance.MainMenu();
    }

    private void StarPopper(int level, int starCount)
    {
        if (starCount == 3)
        {
            _levelsToUnlock[level].transform.GetChild(0).GetChild(2).GetChild(0).gameObject.SetActive(true);
            _levelsToUnlock[level].transform.GetChild(0).GetChild(2).GetChild(1).gameObject.SetActive(true);
            _levelsToUnlock[level].transform.GetChild(0).GetChild(2).GetChild(2).gameObject.SetActive(true);
        }
        if (starCount == 2)
        {
            _levelsToUnlock[level].transform.GetChild(0).GetChild(2).GetChild(0).gameObject.SetActive(true);
            _levelsToUnlock[level].transform.GetChild(0).GetChild(2).GetChild(1).gameObject.SetActive(true);
            _levelsToUnlock[level].transform.GetChild(0).GetChild(2).GetChild(2).gameObject.SetActive(false);
        }
        if (starCount == 1)
        {
            _levelsToUnlock[level].transform.GetChild(0).GetChild(2).GetChild(0).gameObject.SetActive(true);
            _levelsToUnlock[level].transform.GetChild(0).GetChild(2).GetChild(1).gameObject.SetActive(false);
            _levelsToUnlock[level].transform.GetChild(0).GetChild(2).GetChild(2).gameObject.SetActive(false);
        }
    }

    private void LoadDictionary()
    {
        _levelCompleteAndStarsGainedDict = SaveManager.Instance.LoadJson();
    }

    public void NextArena()
    {
        _nextAndPreviousArenaButtons[0].interactable = true;
        _nextAndPreviousArenaButtons[1].interactable = false;

        _presentArena = _presentArena + 1;
        int previousArena = _presentArena - 1;
        if (_presentArena == _arena.Count)
        {
            _presentArena = 0;
        }
        _arena[_presentArena].SetActive(true);
        _arena[previousArena].SetActive(false);
    }
    public void PreviousArena()
    {
        _nextAndPreviousArenaButtons[0].interactable = false;
        _nextAndPreviousArenaButtons[1].interactable = true;

        _presentArena = _presentArena - 1;
        int previousArena = _presentArena + 1;
        if (_presentArena < 0)
        {
            _presentArena = _arena.Count - 1;
        }
        _arena[_presentArena].SetActive(true);
        _arena[previousArena].SetActive(false);
    }
    private void FindIfArenaCompleted(int levelCompleted)
    {
        if (levelCompleted % 5 == 0)
        {
            Dictionary<int, int> loadedData = SaveManager.Instance.LoadJson(); // Assuming you have already loaded the data
            totalStars = SaveManager.Instance.GetTotalStars(loadedData, levelCompleted);
            Debug.Log("Total Stars for Current and Previous Four Levels: " + totalStars);
            _isArenaCompleted = true;
            _levelsCompleted = new List<int>();
            for (int i = 0; i < levelCompleted; i++)
            {
                _levelsCompleted.Add(i);
            }
        }
    }

}