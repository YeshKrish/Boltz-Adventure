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
    [SerializeField]
    LevelAndStar _levelAndStar;

    private static LevelSelect instance;

    private static List<int> _previousLevelClearedCount = new List<int>();
    
    //private static List<string> _loadedFile = new List<string> ();
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
        //foreach (KeyValuePair<int, int>  keyValue in LevelAndStar.Instance.LevelAndStarDict)
        //{
        //    Debug.Log(keyValue.Key + " " + keyValue.Value);
        //}

        _levelCompleteAndStarsGainedDict = SaveManager.Instance.LoadJson();
        foreach(KeyValuePair<int, int> load in _levelCompleteAndStarsGainedDict)
        {
            Debug.Log(load);
        }

        foreach (int prev in _previousLevelClearedCount)
        {
            Debug.Log(prev);
        }

        DisableAll();
        if (!PlayerPrefs.HasKey("LevelClearedCount"))
            PlayerPrefs.SetInt("LevelClearedCount", 0);

        int levelClearedCount = PlayerPrefs.GetInt("LevelClearedCount");
        //Debug.Log(levelClearedCount + " " + _previousLevelClearedCount.Contains(levelClearedCount));
        //Checking if it is a new level, if nw adding the levelCleareddCount to previouseLevelCount list
        if (levelClearedCount > 0 && !_previousLevelClearedCount.Contains(levelClearedCount))
        {
            //If LevelSelect screen loads from a Level
            if (PlayerPrefs.GetInt("IsLastSceneMainMenu") == 0)
            {
                //No of stars to be poped up
                if(PlayerPrefs.GetString("CoinsCollected") == "CollectedAll")
                {
                    _levelAndStar.LevelAndStarDict.Add(levelClearedCount - 1, 3);
                    StarPopper(3, levelClearedCount - 1);
                    SaveManager.Instance.SaveJson();
                    
                }
                if(PlayerPrefs.GetString("CoinsCollected") == "Collected Half")
                {
                    _levelAndStar.LevelAndStarDict.Add(levelClearedCount - 1, 2);
                    StarPopper(2, levelClearedCount - 1);
                    SaveManager.Instance.SaveJson();
                }
                if(PlayerPrefs.GetString("CoinsCollected") == "Collected Quater")
                {
                    _levelAndStar.LevelAndStarDict.Add(levelClearedCount - 1, 1);
                    StarPopper(1, levelClearedCount - 1);
                    SaveManager.Instance.SaveJson();
                }


                for (int i = 0; i < levelClearedCount; i++)
                {
                    if(_levelAndStar.LevelAndStarDict != null)
                    {
                        StarPopper(StarValueFetcher(i), i);
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
                if(levelClearedCount == 5)
                {
                    foreach (KeyValuePair<int, int> keyValuePair in _levelCompleteAndStarsGainedDict)
                    {
                        StarPopper(keyValuePair.Value, keyValuePair.Key);
                    }
                    for (int i = 0; i < levelClearedCount; i++)
                    {
                        //if (_levelAndStar.LevelAndStarDict != null)
                        //{
                        //    Debug.Log(i + " " + StarValueFetcher(i));
                        //    StarPopper(StarValueFetcher(i), i);
                        //}
                        _levelsToUnlock[i].interactable = true;
                        _levelsToUnlock[i].transform.GetChild(0).GetChild(1).gameObject.SetActive(false);
                    }
                }
                else
                {
                    foreach (KeyValuePair<int, int> keyValuePair in _levelCompleteAndStarsGainedDict)
                    {
                        StarPopper(keyValuePair.Value, keyValuePair.Key);
                    }
                    for (int i = 0; i < levelClearedCount + 1; i++)
                    {
                        //if (_levelAndStar.LevelAndStarDict != null && i < levelClearedCount)
                        //{
                        //    Debug.Log(i + " " + StarValueFetcher(i));
                        //    StarPopper(StarValueFetcher(i), i);
                        //}
                       
                        _levelsToUnlock[i].interactable = true;
                        _levelsToUnlock[i].transform.GetChild(0).GetChild(1).gameObject.SetActive(false);
                    }
                }
            }
            _previousLevelClearedCount.Add(levelClearedCount);
        }
        else if(levelClearedCount > 0 && _previousLevelClearedCount.Contains(levelClearedCount))
        {
            if (levelClearedCount == 5)
            {
                foreach (KeyValuePair<int, int> keyValuePair in _levelCompleteAndStarsGainedDict)
                {
                    StarPopper(keyValuePair.Value, keyValuePair.Key);
                }
                for (int i = 0; i < levelClearedCount; i++)
                {
                    //if (_levelAndStar.LevelAndStarDict != null)
                    //{
                    //    Debug.Log(i + " " + StarValueFetcher(i));
                    //    StarPopper(StarValueFetcher(i), i);
                    //}
                    _levelsToUnlock[i].interactable = true;
                    _levelsToUnlock[i].transform.GetChild(0).GetChild(1).gameObject.SetActive(false);
                }
            }
            else
            {
                foreach (KeyValuePair<int, int> keyValuePair in _levelCompleteAndStarsGainedDict)
                {
                    StarPopper(keyValuePair.Value, keyValuePair.Key);
                }
                for (int i = 0; i < levelClearedCount + 1; i++)
                {
                    //if (_levelAndStar.LevelAndStarDict != null && i < levelClearedCount)
                    //{
                    //    Debug.Log(i + " " + StarValueFetcher(i));
                    //    StarPopper(StarValueFetcher(i), i);
                    //}
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
        MusicManager.instance.ButtonClickSound();
        SceneManager.LoadScene("MainMenu");
    }

    //Need to work
    //private int StarKeyFetcher()
    //{
    //    if(_levelCompleteAndStarsGainedDict != null)
    //    {
    //        foreach (KeyValuePair<int, int> dictionaryItem in _levelCompleteAndStarsGainedDict)
    //        {
    //            return dictionaryItem.Key;
    //        }
    //    }
    //    return 0;
    //} 
    private int StarValueFetcher(int key)
    {
        return _levelAndStar.LevelAndStarDict[key];
    }

    private void StarPopper(int starCount, int level)
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

}
