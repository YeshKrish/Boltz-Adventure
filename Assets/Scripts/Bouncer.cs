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

    private Vector3 _distanceBetweenBouncerAndBrick;
    private float _verticalDistanceBetweenBouncerAndBrick;

    private void OnEnable()
    {
        PlayerController.Bounce += Bounce;
    }

    private void Start()
    {
        _playerRigidBody = _player.GetComponent<Rigidbody>();
    }

    //Find the Vertical distance betweem the Bouncer and the Nearby Store and find the angle betwwn those and add the Angle with the Vertical Distance
    private void Bounce()
    {
        if (!_isBounced)
        {
            _isBounced = true;
            _verticalDistanceBetweenBouncerAndBrick = Mathf.Abs(transform.position.y - _nearByBrick.position.y);
            _distanceBetweenBouncerAndBrick = (_nearByBrick.position - transform.position).normalized;
            float dot = Vector3.Dot(transform.up, _distanceBetweenBouncerAndBrick);
            float angle = Mathf.Acos(dot);
            Debug.Log(dot + " Dot and distance " + _distanceBetweenBouncerAndBrick + " " + angle);
            _playerRigidBody.AddForce(transform.up * (_verticalDistanceBetweenBouncerAndBrick + angle), ForceMode.Impulse);
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

    private void OnDisable()
    {
        PlayerController.Bounce -= Bounce;
    }

}
