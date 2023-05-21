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

    private int _maxHitFromPlayer = 3;
    private int _hit = 0;
    private GameObject firedBullets;
    public static Vector3 _startPos;
    private int _maxBulltsToBeSpawned = 5;
    public List<GameObject> _bulletsList;
    private bool _canShootAnimationPlay = false;
    private Vector3 _bulletInitalVelocity;
    private Rigidbody _bulletRigidBody;

    private int _noOfBulletsSpawned = 0;
    private float _attack1Delay = 2800f; // Delay time for the first attack type
    private float _attack2Delay = 1200f; // Delay time for the second attack type
    private float _delayBetweenAttack1AndAttack2 = 3000f; // Delay time for the second attack type

    private void OnEnable()
    {
        ProjectileMoveScript.DeactivateAllActiveBullets += DeactivateBullets;
        ShootTrigger.StartShooting += SpawnBulletsOnAInterval;
    }
    private void Start()
    {
        _isAlienDead = false;
        _bulletRigidBody = _bullets.GetComponent<Rigidbody>();
        _bulletInitalVelocity = _bulletRigidBody.velocity;
        _bulletsList = new List<GameObject>();
        for (int i = 0; i < _maxBulltsToBeSpawned; i++)
        {
            GameObject bullet = Instantiate(_bullets);
            bullet.SetActive(false);
            _bulletsList.Add(bullet);
        }

        //SpawnBulletsOnAInterval();
        EndBlockPosition = ProjectileEnd.transform.position;
    }

    private void Update()
    {
        Debug.Log(_canShootAnimationPlay);

        if (_canShootAnimationPlay)
        {
            Animator.SetBool("isShoot", true);
        }
        else
        {
            Animator.SetBool("isShoot", false);
        }

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Animator.SetBool("isHitReceived", true);
            _hit++;
            if(_hit == _maxHitFromPlayer)
            {
                Dead();
            }
        }
    }    
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Animator.SetBool("isHitReceived", false);
        }
    }

    async void SpawnBulletsOnAInterval()
    {
        int called = 0;
        called++;
        Debug.Log(called + "called");
        while (true && !GameManager.instance.IsPlayerDead)
        {
            Debug.Log(_noOfBulletsSpawned);
            //bool bulletsActive = CheckIfBulletsActive();
            //_canShootAnimationPlay = bulletsActive; // Update the _canShootAnimationPlay flag

            float delayTime;

            if (_noOfBulletsSpawned >= 0 && _noOfBulletsSpawned < 3)
            {
                delayTime = _attack1Delay; // First attack type
            }
            else if(_noOfBulletsSpawned == 3)
            {
                delayTime = _delayBetweenAttack1AndAttack2;
            }
            else if (_noOfBulletsSpawned > 3 && _noOfBulletsSpawned <= 6)
            {
                delayTime = _attack2Delay; // Second attack type
            }
            else
            {
                _noOfBulletsSpawned = 0;
                await Task.Delay(3000); // 5-second delay before starting attack 1 again
                continue;// Reset bullet count to 0
            }

            await Task.Delay((int)delayTime);
            _canShootAnimationPlay = true;
            ShootBullet();
        }
    }
    async void ShootBullet()
    {
        await Task.Delay(100);
        if (!GameManager.instance.IsPlayerDead && !_isAlienDead)
        {
            FireBullets();
        }
    }

    //private bool CheckIfBulletsActive()
    //{
    //    foreach (GameObject bullet in _bulletsList)
    //    {
    //        if (bullet.activeSelf)
    //        {
    //            return true;
    //        }
    //    }

    //    return false;
    //}

    private void Dead()
    {
        GameObject _burstEffectWaste = (GameObject) Instantiate(_burstEffect, transform.position, Quaternion.Euler(0f, 90f, 0f));
        Destroy(_burstEffectWaste, 1f);
        MusicManager.instance.MosterDead();
        AllSceneManager.instance.DeactivateObjects(_objectsToDestroy);
        AllSceneManager.instance.ActivateWayPointBasedOnCondition(_movingCube);
        this.gameObject.SetActive(false);
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
        StopShootAnimation();
        _noOfBulletsSpawned++;
        firedBullets =  RetriveBullets();
        //Animator.SetBool("isShoot", true);
        firedBullets.transform.position = _bulletPlace.transform.position;
        firedBullets.transform.rotation = Quaternion.Euler(0f, -90f, 0f);
        //firedBullets = (GameObject)Instantiate(_bullets, _bulletPlace.transform.position, Quaternion.Euler(0f, -90f, 0f));
        _startPos = _bulletPlace.transform.position;
        Rigidbody bulletRigid = firedBullets.GetComponent<Rigidbody>();
        bulletRigid.velocity = _bulletInitalVelocity;
        bulletRigid.AddForce(new Vector3(-1f, 0f, 0f) * _bulletSpeed, ForceMode.VelocityChange);
        Debug.Log(firedBullets.transform.position.x + " " + (_startPos.x - 5));

    }

    async void StopShootAnimation()
    {
        await Task.Delay(200);
        _canShootAnimationPlay = false;
    }
    private GameObject RetriveBullets()
    {
        foreach (GameObject bullet in _bulletsList)
        {
            if (bullet.activeSelf == false && !_isAlienDead)
            {
                bullet.SetActive(true);
                return bullet;
            }
        }
        return null;
    }

    private void DeactivateBullets()
    {
        foreach (GameObject bullet in _bulletsList)
        {
            if(bullet.activeSelf == true)
            {
                bullet.SetActive(false);
            }
        }
    }

    private void OnDisable()
    {
        ProjectileMoveScript.DeactivateAllActiveBullets -= DeactivateBullets;
        _isAlienDead = false;
        ShootTrigger.StartShooting -= SpawnBulletsOnAInterval;
    }
}
