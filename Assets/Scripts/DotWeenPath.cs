using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DotWeenPath : MonoBehaviour
{
    [SerializeField]
    private PathType _pathType = PathType.Linear;
    [SerializeField]
    private GameObject[] _wayPoints;

    private Vector3[] _wayPointVector;

    private void Start()
    {
        _wayPointVector = new Vector3[4];
        for (int i = 0; i < _wayPoints.Length; i++)
        {
            _wayPointVector[i] = _wayPoints[i].transform.position;
        }

        transform.DOPath(_wayPointVector, 4, _pathType);
    }

}
