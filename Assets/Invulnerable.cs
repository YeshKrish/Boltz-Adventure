using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Invulnerable : MonoBehaviour
{
    public static bool _isPlayerInInVulnerableArea = false;
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            _isPlayerInInVulnerableArea = true; 
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            _isPlayerInInVulnerableArea = false;
        }
    }
}
