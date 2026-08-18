using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Customize : MonoBehaviour
{
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
        for (int i = 0; i < SpotLights.Count; i++)
        {
            SpotLights[i].SetActive(i == ballId);
        }
    }

}
