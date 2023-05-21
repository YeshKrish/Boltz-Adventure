using System.Threading.Tasks;
using System;
using UnityEngine;

public class ShootTrigger : MonoBehaviour
{
    [SerializeField]
    private GameObject _shootEffect;

    private bool _canBulletsSpawn = false;
    public static bool _isPlayerInShootingArea = false;

    public static event Action StartShooting;

    private void OnTriggerEnter(Collider other)
    {
        _canBulletsSpawn = true;
        _isPlayerInShootingArea = true;
        Debug.Log("is" + _isPlayerInShootingArea);
        if (other.gameObject.CompareTag("Player"))
        {
            //_shootEffect.SetActive(true);
            if (_canBulletsSpawn)
            {
                StartShooting?.Invoke();
                //StartBulletSpawning();
            }

        }
    }

    async void StartBulletSpawning()
    {
        while (true)
        {
            await Task.Delay(3000);
            StartShooting?.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        _canBulletsSpawn = false;
        _isPlayerInShootingArea = false;
        //_shootEffect.SetActive(false);
    }

}
