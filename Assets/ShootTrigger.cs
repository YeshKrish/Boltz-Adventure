using System;
using UnityEngine;

public class ShootTrigger : MonoBehaviour
{
    [SerializeField]
    private GameObject _shootEffect;

    public static event Action StartShooting;

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            _shootEffect.SetActive(true);
            StartShooting?.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        _shootEffect.SetActive(false);
    }

}
