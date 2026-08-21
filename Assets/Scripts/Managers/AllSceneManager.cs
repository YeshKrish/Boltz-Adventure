using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.Serialization;

public class AllSceneManager : MonoBehaviour
{
    public static AllSceneManager Instance;
    [FormerlySerializedAs("_audioSprites")]
    [Tooltip("Speaker icons: 0 and 1 for the menu, 2 and 3 for in game.")]
    public Sprite[] AudioSprites;

    private string _previousScene; 

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
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

    public void ActivateWayPointBasedOnCondition(List<GameObject> wayPoinObjectToActivate)
    {
        foreach (GameObject _gameObj in wayPoinObjectToActivate)
        {
            Debug.Log(_gameObj.name);
            _gameObj.GetComponent<WayPointFollower>().enabled = true;
        }
    }
}
