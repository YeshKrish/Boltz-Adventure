using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Runtime.InteropServices;

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
    private static Dictionary<int, int> _levelCompleteAndStarsGainedDict = new Dictionary<int, int>();

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
        DisableAll();
        if (!PlayerPrefs.HasKey("LevelClearedCount"))
            PlayerPrefs.SetInt("LevelClearedCount", 0);

        int levelClearedCount = PlayerPrefs.GetInt("LevelClearedCount");
        Debug.Log(levelClearedCount + " " + _previousLevelClearedCount.Contains(levelClearedCount));
        //Checking if it is a new level, if nw adding the levelCleareddCount to previouseLevelCount list
        if (levelClearedCount > 0 && !_previousLevelClearedCount.Contains(levelClearedCount))
        {
            //If LevelSelect screen loads from a Level
            if (PlayerPrefs.GetInt("IsLastSceneMainMenu") == 0)
            {
                //No of stars to be poped up
                if(PlayerPrefs.GetString("CoinsCollected") == "CollectedAll")
                {
                    Debug.Log("Three" + " " + _levelsToUnlock[levelClearedCount - 1].transform.GetChild(0).GetChild(2).GetChild(0).gameObject.name);
                    _levelCompleteAndStarsGainedDict.Add(levelClearedCount - 1, 3);
                    _levelsToUnlock[levelClearedCount-1].transform.GetChild(0).GetChild(2).GetChild(0).gameObject.SetActive(true);
                    _levelsToUnlock[levelClearedCount-1].transform.GetChild(0).GetChild(2).GetChild(1).gameObject.SetActive(true);
                    _levelsToUnlock[levelClearedCount-1].transform.GetChild(0).GetChild(2).GetChild(2).gameObject.SetActive(true);
                }
                if(PlayerPrefs.GetString("CoinsCollected") == "Collected Half")
                {
                    Debug.Log("Two" + " " + _levelsToUnlock[levelClearedCount - 1].transform.GetChild(0).GetChild(2).GetChild(0).gameObject.name);
                    _levelCompleteAndStarsGainedDict.Add(levelClearedCount - 1, 2);
                    _levelsToUnlock[levelClearedCount - 1].transform.GetChild(0).GetChild(2).GetChild(0).gameObject.SetActive(true);
                    _levelsToUnlock[levelClearedCount - 1].transform.GetChild(0).GetChild(2).GetChild(1).gameObject.SetActive(true);
                    _levelsToUnlock[levelClearedCount - 1].transform.GetChild(0).GetChild(2).GetChild(2).gameObject.SetActive(false);
                }
                if(PlayerPrefs.GetString("CoinsCollected") == "Collected Quater")
                {
                    Debug.Log("One" + " " + _levelsToUnlock[levelClearedCount - 1].transform.GetChild(0).GetChild(2).GetChild(0).gameObject.name);
                    _levelCompleteAndStarsGainedDict.Add(levelClearedCount - 1, 1);
                    _levelsToUnlock[levelClearedCount - 1].transform.GetChild(0).GetChild(2).GetChild(0).gameObject.SetActive(true);
                    _levelsToUnlock[levelClearedCount - 1].transform.GetChild(0).GetChild(2).GetChild(1).gameObject.SetActive(false);
                    _levelsToUnlock[levelClearedCount - 1].transform.GetChild(0).GetChild(2).GetChild(2).gameObject.SetActive(false);
                }


                for (int i = 0; i < levelClearedCount; i++)
                {
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
                    for (int i = 0; i < levelClearedCount; i++)
                    {
                        _levelsToUnlock[i].interactable = true;
                        _levelsToUnlock[i].transform.GetChild(0).GetChild(1).gameObject.SetActive(false);
                    }
                }
                else
                {
                    for (int i = 0; i < levelClearedCount + 1; i++)
                    {
                        _levelsToUnlock[i].interactable = true;
                        _levelsToUnlock[i].transform.GetChild(0).GetChild(1).gameObject.SetActive(false);
                    }
                }
            }
            _previousLevelClearedCount.Add(levelClearedCount);

            foreach (int levels in _previousLevelClearedCount)
            {

                Debug.Log(levels);
            }
        }
        else if(levelClearedCount > 0 && _previousLevelClearedCount.Contains(levelClearedCount))
        {
            if (levelClearedCount == 5)
            {
                for (int i = 0; i < levelClearedCount; i++)
                {
                    _levelsToUnlock[i].interactable = true;
                    _levelsToUnlock[i].transform.GetChild(0).GetChild(1).gameObject.SetActive(false);
                }
            }
            else
            {
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
        MusicManager.instance.ButtonClickSound();
        SceneManager.LoadScene("MainMenu");
    }

    //Need to work
    private void StarFetcher(int levelCompletedCount)
    {
        foreach (KeyValuePair<int, int> dictionaryItem in _levelCompleteAndStarsGainedDict)
        {

        }
    }
}
