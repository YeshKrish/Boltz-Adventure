using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public GameObject QuestTextObj;

    [SerializeField]
    private TMP_Text _coinText;

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

    private void Start()
    {
        _coinText.text = Item.quatity.ToString();
    }

    public void UpdateScoreText()
    {
        Item.quatity = Item.quatity + 1;
        if (Item.quatity < 10)
        {
            _coinText.text = "0" + Item.quatity.ToString();
        }
        else
        {
            _coinText.text = Item.quatity.ToString();
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
