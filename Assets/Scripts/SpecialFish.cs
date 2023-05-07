using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialFish : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> _objectsToDeactivate;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            AllSceneManager.instance.DeactivateObjects(_objectsToDeactivate);
        }
    }
}
