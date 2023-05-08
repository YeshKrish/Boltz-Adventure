using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CharacterAnimatorParamId
{
    public static readonly int BrickFalling = Animator.StringToHash("BrickFall");
}

public class FallingBricks : MonoBehaviour
{
    private Animator _animator;

    private float _currentTime;
    private bool _isPlatformBurst;
    private bool _hasfallen = false;
    private Vector3 _intialPos;

    private void Start()
    {
        _animator = GetComponent<Animator>();
    }


    private void Update()
    {
        _currentTime += Time.deltaTime;
        _isPlatformBurst = Mathf.FloorToInt(Time.time) % 3 == 0;

        _intialPos = transform.position;

        if(transform.position.y < _intialPos.y - 2f)
        {
            _hasfallen = true;
        }
        else
        {
            _hasfallen = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            BrickStartFalling();
        }
    }

    private void BrickStartFalling()
    {
        if(_isPlatformBurst && !_hasfallen)
        {
            _animator.Play(CharacterAnimatorParamId.BrickFalling);
        }
    }
}
