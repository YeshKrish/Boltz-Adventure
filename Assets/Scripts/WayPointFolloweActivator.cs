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
}
