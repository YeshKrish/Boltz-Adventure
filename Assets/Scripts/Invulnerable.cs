using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Invulnerable : MonoBehaviour
{
    public static bool IsPlayerInInvulnerableArea = false;

    [SerializeField]
    private GameObject _hitMyFace;

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            IsPlayerInInvulnerableArea = true; 
            _hitMyFace.SetActive(true);

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            IsPlayerInInvulnerableArea = false;
            _hitMyFace.SetActive(false);
        }
    }
}
