using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Boltz.Save;

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
    [SerializeField]
    private GameObject _OwlTextPrompt;
    [SerializeField]
    private AnimationClip _owlMoveAnim;

    private bool _allStarsCollected = false;

    private static LevelSelect instance;

    private static List<int> _previousLevelClearedCount = new List<int>();

    private Dictionary<int, int> _levelCompleteAndStarsGainedDict = new Dictionary<int, int>();

    private int _presentArena = 0;

    //total Arena stars
    private int _totalArenaStars = 15;
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

        if (SaveService.IsOwlDisappearedOnce)
        {
            _owl.SetActive(false);
        }
    }

    private void Start()
    {
        Debug.Log("total" + totalStars);
        LoadDictionary();

        DisableAll();

        int levelClearedCount = SaveService.ClearedCount;

        //Checking if it is a new level, if nw adding the levelCleareddCount to previouseLevelCount list
        if (levelClearedCount > 0 && !_previousLevelClearedCount.Contains(levelClearedCount))
        {
            //If LevelSelect screen loads from a Level
            if (!GameSession.CameFromMainMenu)
            {
                //No of stars to be poped up
                int startsColected = GameSession.LastRunStars;
                if (startsColected > 0)
                {
                    StarPopper(levelClearedCount - 1, startsColected);
                }

                LoadDictionary();

                for (int i = 0; i < levelClearedCount; i++)
                {
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
                    if (_allStarsCollected)
                    {
                        SaveService.IsOwlDisappearedOnce = true;
                        _presentArena = 1;
                        _nextAndPreviousArenaButtons[0].interactable = true;
                        _nextAndPreviousArenaButtons[1].interactable = false;
                        ArenaCompletionAnimationAndUnlockLogic(levelClearedCount);
                    }
                }
            }
            //If LevelSelect screen loads from a Menu
            else
            {

                //if (_allStarsCollected && SaveService.IsOwlDisappearedOnce)
                //{
                //    ArenaCompletionAnimationAndUnlockLogic(levelClearedCount);
                //}

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
                    FindIfArenaCompleted(levelClearedCount);
                    Debug.Log("1");
                    foreach (KeyValuePair<int, int> keyValuePair in _levelCompleteAndStarsGainedDict)
                    {
                        StarPopper(keyValuePair.Key, keyValuePair.Value);
                    }
                    if (_allStarsCollected)
                    {
                        Debug.Log("All");
                        for (int i = 0; i <= levelClearedCount; i++)
                        {
                            Debug.Log("i" + i);
                            _levelsToUnlock[i].interactable = true;
                            _levelsToUnlock[i].transform.GetChild(0).GetChild(1).gameObject.SetActive(false);
                        }
                    }
                    else
                    {
                        Debug.Log("NotAll");
                        for (int i = 0; i < levelClearedCount; i++)
                        {
                            Debug.Log("i" + i);
                            _levelsToUnlock[i].interactable = true;
                            _levelsToUnlock[i].transform.GetChild(0).GetChild(1).gameObject.SetActive(false);
                        }
                    }
                }
            }
            _previousLevelClearedCount.Add(levelClearedCount);
        }
        else if (levelClearedCount > 0 && _previousLevelClearedCount.Contains(levelClearedCount))
        {
            int startsColected = GameSession.LastRunStars;
            if (startsColected > 0 && !GameSession.CameFromMainMenu)
            {
                LoadDictionary();
            }

            //Stars has to be collected and owl should not have been disappered and it should not be from menu
            if (_allStarsCollected && !SaveService.IsOwlDisappearedOnce && !GameSession.CameFromMainMenu)
            {
                SaveService.IsOwlDisappearedOnce = true;
                _arena[0].SetActive(false);
                _arena[1].SetActive(true);
                _presentArena = 1;
                _nextAndPreviousArenaButtons[0].interactable = true;
                _nextAndPreviousArenaButtons[1].interactable = false;
                ArenaCompletionAnimationAndUnlockLogic(levelClearedCount);
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
                FindIfArenaCompleted(levelClearedCount);
                Debug.Log("2");
                foreach (KeyValuePair<int, int> keyValuePair in _levelCompleteAndStarsGainedDict)
                {
                    StarPopper(keyValuePair.Key, keyValuePair.Value);
                }
                if(_allStarsCollected)
                {
                    Debug.Log("All");
                    for (int i = 0; i <= levelClearedCount; i++)
                    {
                        Debug.Log("i" + i);
                        _levelsToUnlock[i].interactable = true;
                        _levelsToUnlock[i].transform.GetChild(0).GetChild(1).gameObject.SetActive(false);
                    }
                }
                else
                {
                    Debug.Log("NotAll");
                    for (int i = 0; i < levelClearedCount; i++)
                    {
                        Debug.Log("i" + i);
                        _levelsToUnlock[i].interactable = true;
                        _levelsToUnlock[i].transform.GetChild(0).GetChild(1).gameObject.SetActive(false);
                    }
                }
            }
        }
        else if (levelClearedCount == 0)
        {
            _levelsToUnlock[levelClearedCount].interactable = true;
            _levelsToUnlock[levelClearedCount].transform.GetChild(0).GetChild(1).gameObject.SetActive(false);
        }
        
        if (_arena[0].activeSelf)
        {
            Debug.Log("Areana1");
            _presentArena = 0;
            _nextAndPreviousArenaButtons[0].interactable = false;
            _nextAndPreviousArenaButtons[1].interactable = true;
        }
        else if (_arena[1].activeSelf)
        {
            Debug.Log("Areana2");
            _presentArena = 1;
            _nextAndPreviousArenaButtons[0].interactable = true;
            _nextAndPreviousArenaButtons[1].interactable = false;
        }
    }

    private void Update()
    {
        int levelClearedCount = SaveService.ClearedCount;

        FindIfArenaCompleted(levelClearedCount);

        if (_allStarsCollected && GameSession.CameFromMainMenu)
        {
            _owl.SetActive(false);
        }
        if (_allStarsCollected && !SaveService.IsOwlDisappearedOnce && !GameSession.CameFromMainMenu)
        {
            SaveService.IsOwlDisappearedOnce = true;
            Destroy(_OwlTextPrompt);
            DisableOwl();
            _arena[0].SetActive(false);
            _arena[1].SetActive(true);
            ArenaCompletionAnimationAndUnlockLogic(levelClearedCount);
        }

    }

    async void DisableOwl()
    {
        await Task.Delay(4000);
        _owl.SetActive(false);
        if (SaveService.IsOwlDisappearedOnce)
        {
            SaveService.Flush();
        }
    }

    async void ArenaCompletionAnimationAndUnlockLogic(int levelClearedCount)
    {
        _presentArena = 1;
        _nextAndPreviousArenaButtons[0].interactable = true;
        _nextAndPreviousArenaButtons[1].interactable = false;
        await Task.Delay(300);
        _ownDisappearingAnimation.SetBool("isNewArenaUnlocked", true);
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
        _levelCompleteAndStarsGainedDict.Clear();

        for (int ordinal = 0; ordinal < _levelsToUnlock.Length; ordinal++)
        {
            var levelId = LevelId.ForOrdinal(ordinal);
            if (string.IsNullOrEmpty(levelId))
                continue;

            int stars = SaveService.GetStars(levelId);
            if (stars > 0)
                _levelCompleteAndStarsGainedDict[ordinal] = stars;
        }
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
    /// <summary>
    /// Sums the stars for the arena the player is currently looking at, which is the window of five
    /// levels ending at <paramref name="levelCompleted"/>.
    ///
    /// This used to re-read the save file from disk on every call, and Update calls it every frame.
    /// On a fresh install the read returned null and the sum threw, so the level select screen
    /// logged an error and raised an exception once per frame until the first level was finished.
    /// </summary>
    private void FindIfArenaCompleted(int levelCompleted)
    {
        int startLevel = Mathf.Max(levelCompleted - 5, 0);

        totalStars = 0;
        for (int level = startLevel; level <= levelCompleted; level++)
        {
            if (_levelCompleteAndStarsGainedDict.TryGetValue(level, out int stars))
                totalStars += stars;
        }

        if (totalStars == _totalArenaStars)
        {
            _allStarsCollected = true;
        }
    }

    public void ActivateOwlPrompt()
    {
        if (!_OwlTextPrompt.activeSelf)
        {
            _OwlTextPrompt.SetActive(true);
            DeActivateOwlPromptAfterSecs();
        }
    }

    async void DeActivateOwlPromptAfterSecs()
    {
        await Task.Delay(2500);
        _OwlTextPrompt.SetActive(false);
    }
}