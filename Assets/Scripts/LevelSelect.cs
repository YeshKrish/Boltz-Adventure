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

        int levelClearedCount = PlayerPrefs.GetInt("LevelClearedCount");
        for (int i = 0; i < levelClearedCount + 1; i++)
        {
            if (levelClearedCount == 5)
            {
                for (int j = 0; j < levelClearedCount; j++)
                {
                    _levelsToUnlock[j].interactable = true;
                    _levelsToUnlock[j].transform.GetChild(0).GetChild(1).gameObject.SetActive(false);
                }
            }
            else
            {
                _levelsToUnlock[i].interactable = true;
                _levelsToUnlock[i].transform.GetChild(0).GetChild(1).gameObject.SetActive(false);
            }
        }
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
    public void MainMenu()
    {
        MusicManager.instance.ButtonClickSound();
        SceneManager.LoadScene("MainMenu");
    }

}
