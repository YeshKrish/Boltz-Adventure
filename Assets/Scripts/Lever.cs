using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Lever : MonoBehaviour
{
    [SerializeField]
    private GameObject[] _gameObjectsToActivate;
    [SerializeField]
    private Animator _leverOn;

    public static event Action DoorOpen;

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.gameObject.name);
        if (collision.gameObject.CompareTag("Player"))
        {
            _leverOn.SetBool("canLevelOn", true);
            foreach (GameObject _gameObj in _gameObjectsToActivate)
            {
                _gameObj.GetComponent<WayPointFollower>().enabled = true;
            }
            
            DoorOpen?.Invoke();
        }
    }
}
