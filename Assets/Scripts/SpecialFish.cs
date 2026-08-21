using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialFish : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> _objectsToDeactivate;

    public static bool IsFishDead = false;

    private void Start()
    {
        IsFishDead = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            MusicManager.Instance.FishDyingSound();
            IsFishDead=true;
            Debug.Log("Comeon");
            gameObject.SetActive(false);
            AllSceneManager.Instance.DeactivateObjects(_objectsToDeactivate);
        }
    }
}
