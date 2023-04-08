using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomizeManager : MonoBehaviour
{
    [SerializeField]
    private GameObject _player;

    private GameObject _defaultBall;
    [SerializeField]
    private ChooseBall _ballPool;

    public static CustomizeManager Instance;

    public List<GameObject> SpotLights = new List<GameObject>();

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }

        _defaultBall = _ballPool.BallPool[0];
        if (_ballPool.PreviousBall != null)
        {
            _ballPool.PreviousBall.SetActive(true);
        }
        else
        {
            _defaultBall.SetActive(true);
        }

        DontDestroyOnLoad(Instance);
    }
    public void ActivateParticularBall(int ballId)
    {
        Debug.Log(ballId);
        for (int i = 0; i < _ballPool.BallPool.Length; i++)
        {
            if(i == ballId)
            {
                SpotLightChoose(i);
                _ballPool.BallPool[i].SetActive(true);
                _ballPool.PreviousBall = _ballPool.BallPool[i];
                //if(i == 0)
                //{
                //    _defaultBall.SetActive(true);

                //}
                //else
                //{
                //    _ballPool.BallPool[i].SetActive(true);
                //    _defaultBall.SetActive(false);
                //}
                //_ballPool.BallPool[i + 1].SetActive(false);
            }
            else
            {
                _ballPool.BallPool[i].SetActive(false);
            }
        }
    }
    public void MainMenu()
    {
        NavigationManager.Instance.MainMenu();
    }

    private void SpotLightChoose(int ballId)
    {
        for (int i = 0; i < _ballPool.SpotLight.Length; i++)
        {
            if(i == ballId)
            {
                _ballPool.SpotLight[i].SetActive(true);
                SpotLights[i].SetActive(true);
            }
            else
            {
                _ballPool.SpotLight[i].SetActive(false);
                SpotLights[i].SetActive(false);
            }
        }
    }
}
