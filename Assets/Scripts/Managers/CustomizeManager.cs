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

    public List<Image> BallImages = new List<Image>();
    

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }

        _defaultBall = _ballPool.BallPool[0];
        _defaultBall.SetActive(true);
        DontDestroyOnLoad(Instance);
    }

    public void ActivateParticularBall(int ballId)
    {
        Debug.Log(ballId);
        for (int i = 0; i < _ballPool.BallPool.Length; i++)
        {
            if(i == ballId)
            {
                _ballPool.BallPool[i].SetActive(true);
                ImageAlphaChange(i);
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

    private void ImageAlphaChange(int ballId)
    {
        for (int i = 0; i < BallImages.Count; i++)
        {
            if(i == ballId)
            {
                Color temp = BallImages[i].color;
                temp.a = 1f;
                BallImages[i].color = temp;
            }
            else
            {
                Color temp = BallImages[i].color;
                temp.a = 0.5f;
                BallImages[i].color = temp;
            }
        }
    }
}
