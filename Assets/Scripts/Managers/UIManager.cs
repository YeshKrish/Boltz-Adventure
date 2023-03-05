using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField]
    private TMP_Text _cointText;

    private int _total = 0;

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

    }

    void Start()
    {
        
    }


    public void UpdateScoreText()
    {
        _total++;
        if(_total < 10)
        {
            _cointText.SetText("0" + _total.ToString());
        }
        else
        {
            _cointText.SetText(_total.ToString());
        }
    }

    public void GameStart()
    {
        Time.timeScale = 1f;
    }

    public void RetryLevel()
    {
        MusicManager.instance.ButtonClickSound();
        SceneManager.LoadScene(GameManager.instance.GetCurrentScene());
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
