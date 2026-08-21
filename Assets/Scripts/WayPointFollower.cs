using UnityEngine;

/// <summary>
/// Walks an object around a loop of waypoints.
///
/// This used to write transform.position from Update. Everything it moves carries a collider,
/// and moving a collider by its transform makes the physics engine rebuild its acceleration
/// structure without ever giving the body a velocity, so anything standing on it gets no
/// carrying motion and a fast enough mover can pass through the player between frames. It moves
/// a kinematic rigidbody with MovePosition on the physics step now, and the bodies are set to
/// interpolate so they still render smoothly between those steps.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class WayPointFollower : MonoBehaviour
{
    /// <summary>How close counts as having reached a waypoint.</summary>
    private const float ArrivalDistance = 0.1f;

    public GameObject[] wayPoints;

    public float wayPointSpeed = 2f;

    private Rigidbody _rb;
    private int _currentWayPointIndex = 0;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        // Indexing straight into the array is what this did before, so an object left without
        // waypoints threw on its first frame.
        if (wayPoints == null || wayPoints.Length == 0)
        {
            return;
        }

        GameObject target = wayPoints[_currentWayPointIndex];
        if (target == null)
        {
            return;
        }

        Vector3 targetPosition = target.transform.position;

        if (Vector3.Distance(_rb.position, targetPosition) < ArrivalDistance)
        {
            _currentWayPointIndex = (_currentWayPointIndex + 1) % wayPoints.Length;
            return;
        }

        _rb.MovePosition(Vector3.MoveTowards(_rb.position, targetPosition, wayPointSpeed * Time.fixedDeltaTime));
    }
}
