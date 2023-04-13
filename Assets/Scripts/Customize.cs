using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Customize : MonoBehaviour
{
    [SerializeField]
    private GameObject _player;

    [SerializeField]
    private ChooseBall _ballPool;


    public List<GameObject> SpotLights = new List<GameObject>();

    private void Start()
    {
        SpotLightChoose(BallManager.Instance.GetActiveball());
    }

    public void ActivateParticularBall(int ballId)
    {
        Debug.Log(ballId);
        BallManager.Instance.ActivateParticularBall(ballId);
        SpotLightChoose(ballId);
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
