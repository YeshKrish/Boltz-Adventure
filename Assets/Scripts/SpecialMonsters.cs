using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class SpecialMonsters : MonoBehaviour
{
    [SerializeField]
    private GameObject _burstEffect;
    [SerializeField]
    private GameObject _shootEffect; 
    [SerializeField]
    private GameObject _player;    
    [SerializeField]
    private GameObject _triggerPoint;
    [SerializeField]
    private GameObject _bulletPlace;
    [SerializeField]
    private float _bulletSpeed = 5f;
    [SerializeField]
    private GameObject _projectileEnd;
    public static Vector3 EndBlockPosition;


    [Tooltip("From 0% to 100%")]
    public float accuracy;
    private int _maxHitFromPlayer = 3;
    private int _hit = 0;
    private Vector3 offset;

    private void OnEnable()
    {
        ShootTrigger.StartShooting += FireBullets;

    }
    private void Start()
    {
        EndBlockPosition = _projectileEnd.transform.position;
        FireBullets();
    }

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
        GameObject _burstEffectWaste = (GameObject) Instantiate(_burstEffect, transform.position, Quaternion.Euler(0f, 90f, 0f));
        Destroy(_burstEffectWaste, 1f);
        DestroyAlien();
    }
    
    async private void DestroyAlien()
    {
        await Task.Delay(200);
        Destroy(gameObject);
    }

    private void FireBullets()
    {
        GameObject firedBullets = (GameObject)Instantiate(_shootEffect, _bulletPlace.transform.position, Quaternion.Euler(0f, -90f, 0f));
        Vector3 startPos = firedBullets.transform.position;
        Rigidbody bulletRigid = firedBullets.GetComponent<Rigidbody>();
        bulletRigid.AddForce(new Vector3(-1f, 0f, 0f) * _bulletSpeed, ForceMode.VelocityChange);
        if (firedBullets.transform.position.x > startPos.x + 5)
        {
            Destroy(firedBullets, 2f);
        }
    }
    private void OnDisable()
    {
        ShootTrigger.StartShooting -= FireBullets;
    }
}
