using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Runtime.InteropServices;
using System.IO;

public class LevelSelect : MonoBehaviour
{
    [SerializeField]
    private Button[] _levelsToUnlock;
    [SerializeField]
    private GameObject[] _locksToUnlock;
    [SerializeField]
    private GameObject[] _stars;

    private static LevelSelect instance;

    private static List<int> _previousLevelClearedCount = new List<int>();
    
    private Dictionary<int, int> _levelCompleteAndStarsGainedDict = new Dictionary<int, int>();

    private void Awake()
    {
        if(instance == null)
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
        LoadDictionary();

        DisableAll();
        if (!PlayerPrefs.HasKey("LevelClearedCount"))
            PlayerPrefs.SetInt("LevelClearedCount", 0);

        int levelClearedCount = PlayerPrefs.GetInt("LevelClearedCount");

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
                if(PlayerPrefs.GetString("CoinsCollected") == "Collected Half")
                {
                    startsColected = 2;
                    StarPopper(levelClearedCount - 1, 2);
                }
                if(PlayerPrefs.GetString("CoinsCollected") == "Collected Quater")
                {
                    startsColected = 1;
                    StarPopper(levelClearedCount - 1, 1);
                }
                SaveManager.Instance.SaveJson(startsColected, levelClearedCount - 1);
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
                _levelsToUnlock[levelClearedCount].transform.GetChild(0).GetChild(1).gameObject.GetComponent<Animator>().enabled = true;
                 StartCoroutine(DisableLockWithAnimation(levelClearedCount));
            }
            //If LevelSelect screen loads from a Menu
            else
            {
                if(levelClearedCount == 6)
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
        else if(levelClearedCount > 0 && _previousLevelClearedCount.Contains(levelClearedCount))
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
        else if(levelClearedCount == 0)
        {
            _levelsToUnlock[levelClearedCount].interactable = true;
            _levelsToUnlock[levelClearedCount].transform.GetChild(0).GetChild(1).gameObject.SetActive(false);
        }
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
        if(starCount == 3)
        {
            _levelsToUnlock[level].transform.GetChild(0).GetChild(2).GetChild(0).gameObject.SetActive(true);
            _levelsToUnlock[level].transform.GetChild(0).GetChild(2).GetChild(1).gameObject.SetActive(true);
            _levelsToUnlock[level].transform.GetChild(0).GetChild(2).GetChild(2).gameObject.SetActive(true);
        }
        if(starCount == 2)
        {
            _levelsToUnlock[level].transform.GetChild(0).GetChild(2).GetChild(0).gameObject.SetActive(true);
            _levelsToUnlock[level].transform.GetChild(0).GetChild(2).GetChild(1).gameObject.SetActive(true);
            _levelsToUnlock[level].transform.GetChild(0).GetChild(2).GetChild(2).gameObject.SetActive(false);
        }if(starCount == 1)
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

}
