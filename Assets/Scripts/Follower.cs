using PathCreation;
using UnityEngine;

/// <summary>
/// Slides an object along an authored path.
///
/// The only thing in the game using this is the music button on the main menu, which is a
/// Canvas element with an Image and a Button and no collider of any kind. It is not a moving
/// platform and nothing stands on it, so it keeps moving its transform rather than being given
/// a rigidbody, which on a RectTransform would fight the Canvas for control of the layout.
/// </summary>
public class Follower : MonoBehaviour
{
    public PathCreator PathCreation;
    public float Speed = 5;
    public GameObject Setting;

    private float travelledDistance;
    private Vector3 _initialPosition;

    private void Awake()
    {
        _initialPosition = transform.position;
    }

    private void Update()
    {
        if (PathCreation == null || PathCreation.path == null)
        {
            return;
        }

        if (travelledDistance <= PathCreation.path.length - 10)
        {
            travelledDistance += Speed * Time.deltaTime;
        }

        transform.position = PathCreation.path.GetPointAtDistance(travelledDistance);
    }

    private void OnDisable()
    {
        transform.position = _initialPosition;
        travelledDistance = 0;
    }
}
