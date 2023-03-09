using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField]
    private Animator _animator;

    private void Update()
    {
        if (GameManager.instance.IsPlayerDead)
        {
            _animator.SetBool("isPlayerDead", true);
        }
    }
}
