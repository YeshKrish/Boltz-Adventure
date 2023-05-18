using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialFish : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> _objectsToDeactivate;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            MusicManager.instance.FishDyingSound();
            Debug.Log("Comeon");
            gameObject.SetActive(false);
            AllSceneManager.instance.DeactivateObjects(_objectsToDeactivate);
        }
    }
}
