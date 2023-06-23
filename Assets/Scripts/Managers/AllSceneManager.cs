using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class AllSceneManager : MonoBehaviour
{
    public static AllSceneManager instance;
    public Sprite[] _audioSprites;

    [SerializeField]
    private LevelSelectScriptableObject _owlSO;

    private string _previousScene; 

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }

        DontDestroyOnLoad(this);

        //GameOver Level index
        PlayerPrefs.SetInt("GameOverLevel", 7);
    }

    private void Start()
    {
        bool owlTriggered = SaveManager.Instance.GetIsOwlTriggeredOnce();
        Debug.Log("Owwl" + owlTriggered);
        if (owlTriggered)
        {
            Debug.Log("Hiii");
            _owlSO.IsOwlDisappereadOnce = owlTriggered;
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

    private void OnApplicationQuit()
    {
        PlayerPrefs.SetInt("IsMainMenuChnagedAtLeastOnce", 0);
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
