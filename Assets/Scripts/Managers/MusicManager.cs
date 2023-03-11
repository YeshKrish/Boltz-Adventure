using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MusicManager : MonoBehaviour
{
    public static MusicManager instance;
    
    public AudioSource MainMenuAudio;

    [SerializeField]
    private AudioSource _buttonSound;  
    [SerializeField]
    private AudioSource _enemyDyingSound;
    [SerializeField]
    private AudioSource _coinCollectSound;
    [SerializeField]
    private AudioSource _gameMusic;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

        DontDestroyOnLoad(this.gameObject);
    }

    public void ButtonClickSound()
    {
        _buttonSound.Play();
    }

    public void GameMusic()
    {
        _gameMusic.Play();
    }
    public void MainMenuMusicStop()
    {
        MainMenuAudio.Stop();
    }

    public void EnemyDyingSound()
    {
        _enemyDyingSound.Play();
    }
    public void CoinCollectSound()
    {
        _coinCollectSound.Play();
    }

    public void ChangeMainMenuMusic()
    {
        if (_gameMusic.isPlaying)
        {
            _gameMusic.Stop();
            MainMenuAudio.Play();
        }
    }
}
