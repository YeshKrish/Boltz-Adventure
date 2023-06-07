using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Invulnerable : MonoBehaviour
{
    public static bool _isPlayerInInVulnerableArea = false;

    [SerializeField]
    private GameObject _hitMyFace;

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            _isPlayerInInVulnerableArea = true; 
            _hitMyFace.SetActive(true);

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            _isPlayerInInVulnerableArea = false;
            _hitMyFace.SetActive(false);
        }
    }
}
