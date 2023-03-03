using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public GameObject _gameOver;
    public GameObject MainMenu;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

        //MainMenu.transform.GetChild(4).GetComponent<Animation>().Play();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GameStart()
    {
        MainMenu.SetActive(false);
        Time.timeScale = 1f;
    }

    public void RetryLevel()
    {

    }
}
