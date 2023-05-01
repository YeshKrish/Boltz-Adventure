using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SawRotate : MonoBehaviour
{
    void Start()
    {
        LeanTween.rotateAroundLocal(gameObject, Vector3.up, 360f, 0.9f).setRepeat(-1);
    }
}
