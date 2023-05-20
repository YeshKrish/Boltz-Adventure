using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialFish : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> _objectsToDeactivate;

    public static bool _isFishDead = false;

    private void Start()
    {
        _isFishDead = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            MusicManager.instance.FishDyingSound();
            _isFishDead=true;
            Debug.Log("Comeon");
            gameObject.SetActive(false);
            AllSceneManager.instance.DeactivateObjects(_objectsToDeactivate);
        }
    }
}
