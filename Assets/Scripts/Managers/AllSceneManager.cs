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

    private void OnApplicationQuit()
    {
        PlayerPrefs.SetInt("IsMainMenuChnagedAtLeastOnce", 0);
    }

}
