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
    private TMP_Text _cointText;

    private static int _total = 0;

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

    public void UpdateScoreText()
    {
        Item.quatity = Item.quatity + 1;
        //_total++;
        if (Item.quatity < 10)
        {
            _cointText.SetText("0" + Item.quatity.ToString());
        }
        else
        {
            _cointText.SetText(Item.quatity.ToString());
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
