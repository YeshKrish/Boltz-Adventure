using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllSceneManager : MonoBehaviour
{
    public static AllSceneManager Instance;

    //Scriptable Objects
    [SerializeField]
    private ChooseBall _ballPool;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }

        SetPreviousBall();
        DontDestroyOnLoad(this);
    }

    private void SetPreviousBall()
    {
        _ballPool.PreviousBall = _ballPool.BallPool[PlayerPrefs.GetInt("PreviousBall")];
    }

}
