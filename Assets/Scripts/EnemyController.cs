using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField]
    private Animator _animator;

    //private void OnEnable()
    //{
    //    PlayerController.EnemyDead += DeadAnimation;
    //}

    //private void OnDisable()
    //{
    //    PlayerController.EnemyDead -= DeadAnimation;
    //}

    private void Update()
    {
        if (GameManager.instance.IsPlayerDead)
        {
            _animator.SetBool("isPlayerDead", true);
        }
    }

    //private void DeadAnimation()
    //{
    //    if(_animator != null || _enemy != null)
    //    {
    //        _enemy.GetComponentInChildren<MeshCollider>().enabled = false;
    //        _enemy.GetComponent<WayPointFollower>().enabled = false;
    //        _animator.SetBool("isDead", true);
    //        Destroy(this.gameObject, 2f);
    //    }

    //}
}
