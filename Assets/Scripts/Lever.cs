using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Lever : MonoBehaviour
{
    [SerializeField]
    private GameObject[] _wayPointsBricksToActivate; 
    [SerializeField]
    private GameObject[] _gameObjectsToActivate;
    [SerializeField]
    private GameObject[] _gameObjectsToDeActivate;
    [SerializeField]
    private Animator _leverOn;

    public static event Action DoorOpen;

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.gameObject.name);
        if (collision.gameObject.CompareTag("Player"))
        {
            _leverOn.SetBool("canLevelOn", true);
            foreach (GameObject _gameObj in _wayPointsBricksToActivate)
            {
                _gameObj.GetComponent<WayPointFollower>().enabled = true;
            } 
            foreach (GameObject _gameObj in _gameObjectsToDeActivate)
            {
                _gameObj.SetActive(false);
            }
            foreach (GameObject _gameObj in _gameObjectsToActivate)
            {
                _gameObj.SetActive(true);
            }

            DoorOpen?.Invoke();
        }
    }
}

