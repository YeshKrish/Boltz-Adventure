using UnityEngine;
using UnityEngine.SceneManagement;


public class MusicManager : MonoBehaviour
{
    public static MusicManager instance;
    
    public AudioSource[] GameAudios;

    [SerializeField]
    private AudioSource _buttonSound;  
    [SerializeField]
    private AudioSource _enemyDyingSound;
    [SerializeField]
    private AudioSource _coinCollectSound;
    [SerializeField]
    private AudioSource _fishHitSound;
    [SerializeField]
    private AudioSource _monsterDeadSound;    
    [SerializeField]
    private AudioSource _jumpSound;    
    [SerializeField]
    private AudioSource _springSound;

    public bool _isGameAudioMuted = false;

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
        if (!_isGameAudioMuted)
        {
            GameAudios[1].Play();
        }
    }
    public void MainMenuMusicStop()
    {
        GameAudios[0].Stop();
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
        if (GameAudios[1].isPlaying)
        {
            GameAudios[1].Stop();
            GameAudios[0].Play();
        }
    }

    public void MuteOrUmuteGameAudio()
    {
        if (!_isGameAudioMuted)
        {
            _isGameAudioMuted = true;
            for(int i = 0; i < GameAudios.Length; i++)
            {
                if (GameAudios[i].isPlaying)
                {
                    GameAudios[i].Pause();
                }
            }
        }
        else if (_isGameAudioMuted)
        {
            _isGameAudioMuted = false;
            if(SceneManager.GetActiveScene().name == "MainMenu")
            {
                GameAudios[0].UnPause();
            }
            else
            {
                GameAudios[1].Play();
            }
        }
    }

    public void FishDyingSound()
    {
        _fishHitSound.Play();
    }

    public void MosterDead()
    {
        _monsterDeadSound.Play();
    }

    public void JumpSound()
    {
        _jumpSound.Play();
    }   
    public void SpringSound()
    {
        _springSound.Play();
    }
}
