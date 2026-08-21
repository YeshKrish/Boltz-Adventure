using System.Collections;
using UnityEngine;

/// <summary>
/// A platform that drops out from under the player a moment after being stood on.
///
/// None of the three conditions that used to gate this worked. The starting position was
/// reassigned from the current position every frame and then compared against itself, so the
/// "has already fallen" flag was always false. The burst flag was an integer modulo one, which
/// is zero for every integer, so it was always true. And the timer it checked counted up from
/// the start of the level rather than from being stood on, so the brick's behaviour depended on
/// how long the player had been in the level rather than on the brick. What survived was a
/// brick that fell one second after contact, provided the level had been running a second, with
/// a fresh one second timer started on every frame of that contact.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class FallingBricks : MonoBehaviour
{
    private static readonly int CanFallParam = Animator.StringToHash("canFall");

    [SerializeField]
    [Tooltip("Seconds between the player landing on the brick and the brick starting to drop.")]
    private float _fallDelay = 1f;

    private Animator _animator;
    private Rigidbody _rb;
    private WayPointFollower _patrol;
    private bool _isFalling;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody>();
        _patrol = GetComponent<WayPointFollower>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_isFalling || !collision.gameObject.CompareTag("Player"))
        {
            return;
        }

        _isFalling = true;

        if (_animator != null)
        {
            _animator.SetBool(CanFallParam, true);
        }

        StartCoroutine(Fall());
    }

    private IEnumerator Fall()
    {
        yield return new WaitForSeconds(_fallDelay);

        // Three of these bricks also patrol a waypoint loop. That loop drives the body with
        // MovePosition, which would go on steering the brick along the path while gravity was
        // supposed to be taking it, so the patrol stops before the body becomes dynamic.
        if (_patrol != null)
        {
            _patrol.enabled = false;
        }

        _rb.isKinematic = false;
        _rb.useGravity = true;
    }
}
