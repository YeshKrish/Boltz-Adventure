using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Lever : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> _wayPointsBricksToActivate;   
    [SerializeField]
    private List<GameObject> _gameObjectsToActivate;
    [SerializeField]
    private List<GameObject> _gameObjectsToDeActivate;
    [SerializeField]
    private Animator _leverOn;

    public static event Action DoorOpen;

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.gameObject.name);
        if (collision.gameObject.CompareTag("Player"))
        {
            _leverOn.SetBool("canLevelOn", true);
            if(PlayerPrefs.GetInt("Current Level") == 5)
            {
                WayPointFolloweActivator.Instance.ActivateWayPointBasedOnCondition(_wayPointsBricksToActivate);

            }
            AllSceneManager.instance.DeactivateObjects(_gameObjectsToDeActivate);
            AllSceneManager.instance.ActivateObjects(_gameObjectsToActivate);

            DoorOpen?.Invoke();
        }
    }
}

