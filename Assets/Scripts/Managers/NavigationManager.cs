using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NavigationManager : MonoBehaviour
{

    public static NavigationManager Instance;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }

        DontDestroyOnLoad(this);
    }

    public void MainMenu()
    {
        MusicManager.instance.ButtonClickSound();
        SceneManager.LoadScene("MainMenu");
    }
}
