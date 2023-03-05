using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelSelect : MonoBehaviour
{
    [SerializeField]
    private Button[] _levelsToUnlock;

    private void Start()
    {
        DisableAll();
        if (!PlayerPrefs.HasKey("LevelClearedCount"))
            PlayerPrefs.SetInt("LevelClearedCount", 0);

        int levelCleared = PlayerPrefs.GetInt("LevelClearedCount");
        for(int i = 0; i < levelCleared+1; i++)
        {
            _levelsToUnlock[i].interactable = true;
            _levelsToUnlock[i].GetComponent<Transform>().GetChild(0).transform.GetChild(1).GetComponent<Transform>().gameObject.SetActive(false);
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

}
