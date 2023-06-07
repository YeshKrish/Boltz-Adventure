using System.Collections.Generic;
using System;
using UnityEngine;

public class ShootTrigger : MonoBehaviour
{
    [SerializeField]
    private GameObject _shootEffect;
    [SerializeField]
    private List<GameObject> _boundry = new List<GameObject>();

    private bool _canBulletsSpawn = false;
    public static bool _isPlayerInShootingArea = false;

    public static event Action StartShooting;

    private void Start()
    {
        _canBulletsSpawn = false;
        _isPlayerInShootingArea = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        _canBulletsSpawn = true;
        _isPlayerInShootingArea = true;
        Debug.Log("is" + _isPlayerInShootingArea);
        if (other.gameObject.CompareTag("Player"))
        {
            for (int i = 0; i < _boundry.Count; i++)
            {
                _boundry[i].GetComponent<BoxCollider>().isTrigger = false;
            }
            if (_canBulletsSpawn)
            {
                StartShooting?.Invoke();
            }

        }
    }

    private void OnTriggerExit(Collider other)
    {
        _canBulletsSpawn = false;
        _isPlayerInShootingArea = false;
    }

}
