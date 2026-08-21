using System.Collections;
using UnityEngine;

/// <summary>
/// A pad that throws the player upward when they land on it.
/// </summary>
[RequireComponent(typeof(Animator))]
public class Bouncer : MonoBehaviour
{
    private static readonly int CanBounceParam = Animator.StringToHash("canBounce");

    [SerializeField]
    private Animator _bounceAnimator;
    [SerializeField]
    private AnimationClip _bounceAnimationClip;
    [SerializeField]
    private Transform _nearByBrick;
    [SerializeField]
    private GameObject _player;

    private Rigidbody _playerRigidBody;
    private float _bounceImpulse;
    private bool _isBounced;

    private void OnEnable()
    {
        PlayerController.Bounce += Bounce;
    }

    private void Start()
    {
        _playerRigidBody = _player.GetComponent<Rigidbody>();
        _bounceImpulse = CalculateBounceImpulse();
    }

    /// <summary>
    /// The strength this pad has always bounced with: the vertical gap to the reference brick,
    /// plus the angle to that brick in radians, plus two.
    ///
    /// Adding an angle to a distance and then to a bare constant does not mean anything, but it
    /// is the number every one of these pads was tuned around, so it is kept rather than
    /// replaced with a value nobody has felt. It used to be worked out afresh on every bounce,
    /// along with a hypotenuse and a dot product angle that were computed and then discarded.
    /// The pad and its reference brick are both static geometry and the bounce animation only
    /// moves bones below this object, so the result cannot change while the level runs.
    /// </summary>
    private float CalculateBounceImpulse()
    {
        Vector3 toBrick = _nearByBrick.position - transform.position;
        float verticalDistance = Mathf.Abs(transform.position.y - _nearByBrick.position.y);
        float angle = Mathf.Atan2(toBrick.y, toBrick.x);

        return verticalDistance + angle + 2f;
    }

    private void Bounce()
    {
        if (_isBounced)
        {
            return;
        }

        _isBounced = true;

        _playerRigidBody.AddForce(transform.up * _bounceImpulse, ForceMode.Impulse);
        _bounceAnimator.SetBool(CanBounceParam, true);

        StartCoroutine(IdleState());
    }

    private IEnumerator IdleState()
    {
        yield return new WaitForSeconds(_bounceAnimationClip.length);

        _bounceAnimator.SetBool(CanBounceParam, false);
        _isBounced = false;
    }

    private void OnDisable()
    {
        PlayerController.Bounce -= Bounce;
    }
}
