using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MusicManager : MonoBehaviour
{
    public static MusicManager instance;

    public AudioSource MainMenuAudio;

    private bool _audioMute = false;

    [SerializeField]
    private Sprite[] _audioSprites;
    [SerializeField]
    public Image _musicImage;

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

        MainMenuAudio.Play();
    }

    public void MuteAudio()
    {
        
        if (_audioMute)
        {
            _audioMute = false;
            _musicImage.sprite = _audioSprites[0];
            MainMenuAudio.Play();
        }
        else
        {
            _audioMute = true;
            _musicImage.sprite = _audioSprites[1];
            MainMenuAudio.Stop();
        }
    }

}
