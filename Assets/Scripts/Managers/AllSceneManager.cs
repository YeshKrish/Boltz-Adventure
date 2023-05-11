using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class AllSceneManager : MonoBehaviour
{
    public static AllSceneManager instance;
    public Sprite[] _audioSprites;

    private string _previousScene; 

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }

        DontDestroyOnLoad(this);

        //GameOver Level index
        PlayerPrefs.SetInt("GameOverLevel", 6);
    }

     //Activate Objects
    public void ActivateObjects(List<GameObject> gameObjToActivate)
    {
        if(gameObjToActivate.Count > 0)
        {
            for(int i = 0; i < gameObjToActivate.Count; i++)
            {
                gameObjToActivate[i].SetActive(true);
            }
        }
    }

    //Deactivate Objects
    public void DeactivateObjects(List<GameObject> gameObjToDeactivate)
    {
        if(gameObjToDeactivate.Count > 0)
        {
            for (int i = 0; i < gameObjToDeactivate.Count; i++)
            {
                gameObjToDeactivate[i].SetActive(false);
            }
        }
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.SetInt("IsMainMenuChnagedAtLeastOnce", 0);
    }
}
