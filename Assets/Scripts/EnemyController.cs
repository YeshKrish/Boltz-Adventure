using System;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField]
    private Animator _animator;

    private void OnEnable()
    {
        PlayerController.KilledByEnemy += EnemyDance;
    }

    private void Start()
    {
        _animator.SetBool("isPlayerDead", false);
    }

    private void EnemyDance()
    {
        _animator.SetBool("isPlayerDead", true);
    }

    private void OnDisable()
    {
        PlayerController.KilledByEnemy -= EnemyDance;
    }
}
