using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialMonsters : MonoBehaviour
{
    [SerializeField]
    private GameObject _burstEffect;

    private int _maxHitFromPlayer = 3;
    private int _hit;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            _hit++;
            if(_hit == _maxHitFromPlayer)
            {
                Dead();
            }
        }
    }

    private void Dead()
    {
        GameObject _burstEffectWaste = (GameObject) Instantiate(_burstEffect, transform.position, transform.rotation);
        Destroy(_burstEffectWaste, 5f);
        Destroy(gameObject);
    }
}
