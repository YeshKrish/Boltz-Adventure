using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Bouncer : MonoBehaviour
{
    [SerializeField]
    private Animator _bounceAnimator;
    [SerializeField]
    private AnimationClip _bounceAnimationClip;
    [SerializeField]
    private Transform _nearByBrick;
    [SerializeField]
    private GameObject _player;

    private Rigidbody _playerRigidBody;

    private bool _isBounced = false;

    private float _distanceBetweenBouncerAndBrick;

    private void OnEnable()
    {
        PlayerController.Bounce += Bounce;
    }

    private void Start()
    {
        _playerRigidBody = _player.GetComponent<Rigidbody>();
    }

    private void Bounce()
    {
        if (!_isBounced)
        {
            _isBounced = true;
            Debug.Log(_distanceBetweenBouncerAndBrick + " " + transform.up);
            _playerRigidBody.AddForce(transform.up * (_distanceBetweenBouncerAndBrick+1), ForceMode.Impulse);
            _bounceAnimator.SetBool("canBounce", true);
            StartCoroutine(IdleState());
        }
    }

    IEnumerator IdleState()
    {
        yield return new WaitForSeconds(_bounceAnimationClip.length);
        _bounceAnimator.SetBool("canBounce", false);
        _isBounced = false;
    }

    private void Update()
    {
        _distanceBetweenBouncerAndBrick = Mathf.Abs(transform.position.y - _nearByBrick.position.y);
    }
    private void OnDisable()
    {
        PlayerController.Bounce -= Bounce;
    }

}
