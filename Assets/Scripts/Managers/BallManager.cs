using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallManager : MonoBehaviour
{
    public static BallManager Instance;

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
        _ballPool.PreviousBall.SetActive(true);
    }

    public void ActivateParticularBall(int ballId)
    {
        for (int i = 0; i < _ballPool.BallPool.Length; i++)
        {
            if (i == ballId)
            {
                _ballPool.BallPool[i].SetActive(true);
                _ballPool.PreviousBall = _ballPool.BallPool[i];
                SetPreviousBallName(_ballPool.PreviousBall.gameObject.name);
            }
            else
            {
                _ballPool.BallPool[i].SetActive(false);
            }
        }
    }

    private void SetPreviousBallName(string ballName)
    {
        if (ballName == "robot_ball")
        {
            PlayerPrefs.SetInt("PreviousBall", 0);
        }
        if (ballName == "rocket ball")
        {
            PlayerPrefs.SetInt("PreviousBall", 1);
        }
        if (ballName == "poke bola")
        {
            PlayerPrefs.SetInt("PreviousBall", 2);
        }
        if (ballName == "FootBall")
        {
            PlayerPrefs.SetInt("PreviousBall", 3);
        }
    }

    public int GetActiveball()
    {
        if (_ballPool.BallPool.Length > 0)
        {
            for (int i = 0; i < _ballPool.BallPool.Length; i++)
            {
                if (_ballPool.BallPool[i].activeSelf)
                {
                    return i;
                }
            }
        }
        return 0;

    }
}
