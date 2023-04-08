using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "BallPool")]
public class ChooseBall : ScriptableObject
{
    public GameObject[] BallPool;

    public GameObject[] SpotLight;

    public GameObject PreviousBall;
}
