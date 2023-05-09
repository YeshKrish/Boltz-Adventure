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
    }

     //Activate Objects
    public void ActivateObjects(List<GameObject> gameObjToActivate)
    {
        for(int i = 0; i < gameObjToActivate.Count; i++)
        {
            gameObjToActivate[i].SetActive(true);
        }
    }

    //Deactivate Objects
    public void DeactivateObjects(List<GameObject> gameObjToDeactivate)
    {
        for (int i = 0; i < gameObjToDeactivate.Count; i++)
        {
            gameObjToDeactivate[i].SetActive(false);
        }
    }

    public GameObject FindTheActivatedObjectInList(List<GameObject> gameObj)
    {
        for (int i = 0; i < gameObj.Count; i++)
        {
            if (gameObj[i].activeSelf)
            {
                return gameObj[i];
            }
        }
        return null;
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.SetInt("IsMainMenuChnagedAtLeastOnce", 0);
    }
}
