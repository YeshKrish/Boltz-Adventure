using UnityEngine;
using System.Collections.Generic;

public class WayPointFolloweActivator : MonoBehaviour
{
    public static WayPointFolloweActivator Instance;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }

    public void ActivateWayPointBasedOnCondition(List<GameObject> wayPoinObjectToActivate)
    {
        foreach (GameObject _gameObj in wayPoinObjectToActivate)
        {
            _gameObj.GetComponent<WayPointFollower>().enabled = true;
        }
    }
}
