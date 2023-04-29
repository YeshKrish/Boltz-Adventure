using PathCreation;
using UnityEngine;

public class Follower : MonoBehaviour
{
    public PathCreator PathCreation;
    public float Speed = 5;
    private float travelledDistance;
    public GameObject Setting;

    private Vector3 _initialPosition;

    private void Awake()
    {
        _initialPosition = transform.position;
        Debug.Log(travelledDistance);
    }

    private void Update()
    {
        if(travelledDistance <= PathCreation.path.length - 10)
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
