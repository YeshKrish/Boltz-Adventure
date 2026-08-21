using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Boltz.Save;

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
            if (GameSession.CurrentLevelBuildIndex == 5)
            {
                AllSceneManager.Instance.ActivateWayPointBasedOnCondition(_wayPointsBricksToActivate);

            }
            AllSceneManager.Instance.DeactivateObjects(_gameObjectsToDeActivate);
            AllSceneManager.Instance.ActivateObjects(_gameObjectsToActivate);

            DoorOpen?.Invoke();
        }
    }
}

