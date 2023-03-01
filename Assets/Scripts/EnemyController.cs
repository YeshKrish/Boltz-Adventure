using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EnemyController : MonoBehaviour
{
    private Animator _animator;

    private int isDancingHash;
    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
        isDancingHash = Animator.StringToHash("isPlayerDead");
    }

    private void Update()
    {
        if (GameManager.instance.IsPlayerDead)
        {
            _animator.SetBool("isPlayerDead", true);
        }
    }

}
