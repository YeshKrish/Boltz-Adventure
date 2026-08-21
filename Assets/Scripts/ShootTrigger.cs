using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Serialization;

public class ShootTrigger : MonoBehaviour
{
    [SerializeField]
    private GameObject _shootEffect;
    [SerializeField]
    [FormerlySerializedAs("_boundry")]
    [Tooltip("Walls that become solid once the player enters the arena.")]
    private List<GameObject> _boundary = new List<GameObject>();

    private bool _canBulletsSpawn = false;
    public static bool IsPlayerInShootingArea = false;

    public static event Action StartShooting;

    private void Start()
    {
        _canBulletsSpawn = false;
        IsPlayerInShootingArea = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        _canBulletsSpawn = true;
        IsPlayerInShootingArea = true;
        Debug.Log("is" + IsPlayerInShootingArea);
        if (other.gameObject.CompareTag("Player"))
        {
            for (int i = 0; i < _boundary.Count; i++)
            {
                _boundary[i].GetComponent<BoxCollider>().isTrigger = false;
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
        IsPlayerInShootingArea = false;
    }

}
