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

    private static LevelSelect instance;

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

        LevelClearer();

    }

    public void LevelClearer()
    {
        int levelCleared = PlayerPrefs.GetInt("LevelClearedCount");
        for (int i = 0; i < levelCleared + 1; i++)
        {
            _levelsToUnlock[i].interactable = true;
            if(i < levelCleared)
            {
                _levelsToUnlock[i].transform.GetChild(0).GetChild(1).gameObject.SetActive(false);
            }
            else if (_levelsToUnlock[i].interactable == true && i > levelCleared)
            {
                _levelsToUnlock[i].transform.GetChild(0).GetChild(1).gameObject.GetComponent<Animator>().enabled = true;
                if (PlayerPrefs.GetInt("IsLastSceneMainMenu") == 0)
                {
                    StartCoroutine(DisableLockWithAnimation(i));
                }
                else
                {
                    DisableLock(i);
                }
            }

        }
    }

    private void DisableLock(int lockNo)
    {
        _levelsToUnlock[lockNo].transform.GetChild(0).GetChild(1).gameObject.SetActive(false);
    }

    IEnumerator DisableLockWithAnimation(int lockNo)
    {
        yield return new WaitForSeconds(1f);
        _levelsToUnlock[lockNo].transform.GetChild(0).GetChild(1).gameObject.SetActive(false);
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
    }

}
