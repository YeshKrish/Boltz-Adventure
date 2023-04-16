using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllSceneManager : MonoBehaviour
{
    public static AllSceneManager instance;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }

        DontDestroyOnLoad(this);
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.SetInt("IsMainMenuChnagedAtLeastOnce", 0);
    }

}
