using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class SpecialMonsters : MonoBehaviour
{
    [SerializeField]
    private GameObject _burstEffect;
    [SerializeField]
    private GameObject _bullets; 
    [SerializeField]
    private GameObject _player;    
    [SerializeField]
    private GameObject _triggerPoint;
    [SerializeField]
    private GameObject _bulletPlace;
    [SerializeField]
    private float _bulletSpeed = 5f;
    [SerializeField]
    private List<GameObject> _objectsToDestroy;
    [SerializeField]
    private List<GameObject> _movingCube;
    public static bool _isAlienDead;
    public GameObject ProjectileEnd;
    public static Vector3 EndBlockPosition;
    public Animator Animator;


    [Tooltip("From 0% to 100%")]
    public float accuracy;
    private int _maxHitFromPlayer = 3;
    private int _hit = 0;
    private Vector3 offset;
    private GameObject firedBullets;
    private Vector3 _startPos;
    private bool _isBulletDestroyed = false;
    private int _maxBulltsToBeSpawned = 5;
    public List<GameObject> _bulletsList;

    private float _bulletSpawnedTime;
    private float _bulletDestroyedTime;
    private Vector3 _bulletInitalVelocity;
    private Rigidbody _bulletRigidBody;

    private int _noOfBulletsSpawned = 0;
    private bool attackType1 = false;
    private bool attackType2 = false;

    private void OnEnable()
    {
        ShootTrigger.StartShooting += FireBullets;
    }
    private void Start()
    {
        _bulletRigidBody = _bullets.GetComponent<Rigidbody>();
        _bulletInitalVelocity = _bulletRigidBody.velocity;
        _bulletsList = new List<GameObject>();
        for (int i = 0; i < _maxBulltsToBeSpawned; i++)
        {
            GameObject bullet = Instantiate(_bullets);
            bullet.SetActive(false);
            _bulletsList.Add(bullet);
        }

        SpawnBulletsOnAInterval();
        EndBlockPosition = ProjectileEnd.transform.position;
    }

    private void Update()
    {
        if (!_isBulletDestroyed)
        {
            if (firedBullets.transform.position.x < _startPos.x - 15)
            {
                firedBullets.SetActive(false);
                _isBulletDestroyed = true;
            }
        }
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

    async void SpawnBulletsOnAInterval()
    {
        FireBullets();
        while (true)
        {
            if(_noOfBulletsSpawned > 0 && _noOfBulletsSpawned <= 3)
            {
                await Task.Delay(1800);
                FireBullets();
            }
            if(_noOfBulletsSpawned > 3 && _noOfBulletsSpawned <= 6)
            {
                await Task.Delay(1200);
                FireBullets();
            }
        }
    }

    private void Dead()
    {
        GameObject _burstEffectWaste = (GameObject) Instantiate(_burstEffect, transform.position, Quaternion.Euler(0f, 90f, 0f));
        Destroy(_burstEffectWaste, 1f);
        AllSceneManager.instance.DeactivateObjects(_objectsToDestroy);
        AllSceneManager.instance.ActivateWayPointBasedOnCondition(_movingCube);
        DestroyAlien();
    }
    
    async private void DestroyAlien()
    {
        await Task.Delay(200);
        _isAlienDead = true;
        Destroy(gameObject);
    }

    private void FireBullets()
    {
        _noOfBulletsSpawned++;
        if(_noOfBulletsSpawned == 7)
        {
            _noOfBulletsSpawned = 0;
        }
        _isBulletDestroyed= false;
        firedBullets =  RetriveBullets();
        Animator.SetBool("isShoot", true);
        firedBullets.transform.position = _bulletPlace.transform.position;
        firedBullets.transform.rotation = Quaternion.Euler(0f, -90f, 0f);
        //firedBullets = (GameObject)Instantiate(_bullets, _bulletPlace.transform.position, Quaternion.Euler(0f, -90f, 0f));
        _startPos = _bulletPlace.transform.position;
        Rigidbody bulletRigid = firedBullets.GetComponent<Rigidbody>();
        bulletRigid.velocity = _bulletInitalVelocity;
        bulletRigid.AddForce(new Vector3(-1f, 0f, 0f) * _bulletSpeed, ForceMode.VelocityChange);
        Debug.Log(firedBullets.transform.position.x + " " + (_startPos.x - 5));

    }

    private GameObject RetriveBullets()
    {
        foreach (GameObject bullet in _bulletsList)
        {
            if (bullet.activeSelf == false)
            {
                bullet.SetActive(true);
                return bullet;
            }
        }
        return null;
    }

    private void OnDisable()
    {
        ShootTrigger.StartShooting -= FireBullets;
    }
}
