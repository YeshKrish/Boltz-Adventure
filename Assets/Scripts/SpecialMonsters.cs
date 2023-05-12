using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialMonsters : MonoBehaviour
{

    private int _maxHitFromPlayer = 3;
    private int _hit;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            
        }
    }
}
