using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The level 6 boss.
///
/// The attack loop, the shot windup, the animation hold and the despawn were all async void
/// methods chained off Task.Delay. Nothing cancelled them. The loop outlived the GameObject it
/// belonged to, kept firing at objects that had been destroyed, and went on reaching through
/// GameManager.instance after a scene change while that static still pointed at a destroyed
/// manager. They are coroutines now, so Unity stops them when this object is disabled or
/// destroyed, and they run on scaled time, which means the boss stops attacking while the game
/// is paused instead of queueing up shots behind the pause screen.
/// </summary>
public class SpecialMonsters : MonoBehaviour
{
    private static readonly int IsShootParam = Animator.StringToHash("isShoot");
    private static readonly int IsHitReceivedParam = Animator.StringToHash("isHitReceived");

    /// <summary>
    /// True from the moment the boss takes its final hit. Read by CameraManager to hand the
    /// camera back and by PlayerController to stop asking for the fight camera.
    /// </summary>
    public static bool _isAlienDead;

    /// <summary>
    /// Where the most recent shot was fired from. Read by ProjectileMoveScript, which retires a
    /// bullet once it has travelled far enough past this point.
    /// </summary>
    public static Vector3 _startPos;

    [SerializeField]
    private GameObject _burstEffect;
    [SerializeField]
    private GameObject _bullets;
    [SerializeField]
    private GameObject _bulletPlace;
    [SerializeField]
    private float _bulletSpeed = 5f;
    [SerializeField]
    private List<GameObject> _objectsToDestroy;
    [SerializeField]
    private List<GameObject> _movingCube;
    [SerializeField]
    private List<GameObject> _enemyHealth;
    [SerializeField]
    private List<GameObject> _boundry = new List<GameObject>();

    [Header("Attack timing, in seconds")]
    [SerializeField]
    [Tooltip("Wait before each of the three opening shots.")]
    private float _openingShotDelay = 2.8f;
    [SerializeField]
    [Tooltip("Wait between the opening volley and the faster closing one.")]
    private float _volleyChangeDelay = 3f;
    [SerializeField]
    [Tooltip("Wait before each shot of the faster closing volley.")]
    private float _closingShotDelay = 1.2f;
    [SerializeField]
    [Tooltip("Rest after a full cycle of shots, before the pattern starts again.")]
    private float _cycleRestDelay = 3f;
    [SerializeField]
    [Tooltip("Gap between the shoot animation starting and the bullet actually leaving.")]
    private float _shotWindup = 0.1f;
    [SerializeField]
    [Tooltip("How long the shoot animation flag stays raised once a shot has gone out.")]
    private float _shootAnimationHold = 0.2f;
    [SerializeField]
    [Tooltip("Gap between the boss dying and its body being removed from the scene.")]
    private float _despawnDelay = 0.2f;

    public Animator Animator;
    public List<GameObject> _bulletsList;

    private int _maxHitFromPlayer = 3;
    private int _hit = 0;
    private int _maxBulltsToBeSpawned = 5;
    private bool _canShootAnimationPlay = false;
    private int _noOfBulletsSpawned = 0;
    private bool _isAttacking = false;

    private void OnEnable()
    {
        ProjectileMoveScript.DeactivateAllActiveBullets += DeactivateBullets;
        ShootTrigger.StartShooting += BeginAttacking;
    }

    private void Start()
    {
        _isAlienDead = false;
        _bulletsList = new List<GameObject>();
        for (int i = 0; i < _maxBulltsToBeSpawned; i++)
        {
            GameObject bullet = Instantiate(_bullets);
            bullet.SetActive(false);
            _bulletsList.Add(bullet);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // The log that used to sit at the top of this method indexed _enemyHealth with the hit
        // count before anything had checked either the tag or the bounds, so the collision after
        // the killing blow threw.
        if (_isAlienDead || !collision.gameObject.CompareTag("Player"))
        {
            return;
        }

        if (Invulnerable._isPlayerInInVulnerableArea || _hit >= _enemyHealth.Count)
        {
            return;
        }

        _enemyHealth[_hit].SetActive(false);
        Animator.SetBool(IsHitReceivedParam, true);
        _hit++;

        if (_hit == _maxHitFromPlayer)
        {
            Dead();
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Animator.SetBool(IsHitReceivedParam, false);
        }
    }

    /// <summary>
    /// The shooting trigger raises its event every time something enters the arena, so walking
    /// back in used to start a second attack loop alongside the first.
    /// </summary>
    private void BeginAttacking()
    {
        if (_isAttacking || _isAlienDead)
        {
            return;
        }

        _isAttacking = true;
        StartCoroutine(Attack());
    }

    private IEnumerator Attack()
    {
        while (!_isAlienDead && !GameManager.instance.IsPlayerDead)
        {
            float delay;

            if (_noOfBulletsSpawned < 3)
            {
                delay = _openingShotDelay;
            }
            else if (_noOfBulletsSpawned == 3)
            {
                delay = _volleyChangeDelay;
            }
            else if (_noOfBulletsSpawned <= 6)
            {
                delay = _closingShotDelay;
            }
            else
            {
                _noOfBulletsSpawned = 0;
                yield return new WaitForSeconds(_cycleRestDelay);
                continue;
            }

            yield return new WaitForSeconds(delay);

            SetShootAnimation(true);

            // Deliberately not waited on. The shot count is raised when the bullet actually
            // leaves, one windup after this point, and the loop reads that count before then.
            // Waiting here would shift every delay in the pattern.
            StartCoroutine(FireAfterWindup());
        }

        _isAttacking = false;
    }

    private IEnumerator FireAfterWindup()
    {
        yield return new WaitForSeconds(_shotWindup);

        if (!_isAlienDead && !GameManager.instance.IsPlayerDead)
        {
            FireBullets();
        }
    }

    private IEnumerator HoldShootAnimation()
    {
        yield return new WaitForSeconds(_shootAnimationHold);
        SetShootAnimation(false);
    }

    private void FireBullets()
    {
        GameObject bullet = RetriveBullets();
        if (bullet == null)
        {
            // Every bullet in the pool is still in flight. Skipping the shot is what the old
            // code meant by returning null here; it went on to dereference that null instead.
            return;
        }

        StartCoroutine(HoldShootAnimation());
        _noOfBulletsSpawned++;

        bullet.transform.position = _bulletPlace.transform.position;
        bullet.transform.rotation = Quaternion.Euler(0f, -90f, 0f);
        _startPos = _bulletPlace.transform.position;

        Rigidbody bulletRigid = bullet.GetComponent<Rigidbody>();

        // This used to assign a field read off the inactive bullet template, which was always
        // zero. Clearing the velocity is what a recycled bullet actually needs.
        bulletRigid.linearVelocity = Vector3.zero;
        bulletRigid.AddForce(new Vector3(-1f, 0f, 0f) * _bulletSpeed, ForceMode.VelocityChange);
    }

    private GameObject RetriveBullets()
    {
        if (_isAlienDead)
        {
            return null;
        }

        foreach (GameObject bullet in _bulletsList)
        {
            if (bullet != null && !bullet.activeSelf)
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
            if (bullet != null && bullet.activeSelf)
            {
                bullet.SetActive(false);
            }
        }
    }

    private void SetShootAnimation(bool isShooting)
    {
        // Written on change. This used to be pushed into the Animator from Update every frame,
        // alongside a log of the same flag.
        if (_canShootAnimationPlay == isShooting)
        {
            return;
        }

        _canShootAnimationPlay = isShooting;
        Animator.SetBool(IsShootParam, isShooting);
    }

    private void Dead()
    {
        GameObject burstEffect = Instantiate(_burstEffect, transform.position, Quaternion.Euler(0f, 90f, 0f));
        Destroy(burstEffect, 1f);

        for (int i = 0; i < _boundry.Count; i++)
        {
            _boundry[i].GetComponent<BoxCollider>().isTrigger = true;
        }

        MusicManager.instance.MosterDead();
        AllSceneManager.instance.DeactivateObjects(_objectsToDestroy);
        AllSceneManager.instance.ActivateWayPointBasedOnCondition(_movingCube);

        // Set here rather than from a delayed callback. OnDisable used to set this back to false
        // on the way past, so whether the camera ever saw it true came down to callback ordering.
        _isAlienDead = true;

        // The pool was five objects instantiated with no parent that nothing ever cleaned up.
        DestroyBullets();

        gameObject.SetActive(false);
        Destroy(gameObject, _despawnDelay);
    }

    private void DestroyBullets()
    {
        for (int i = 0; i < _bulletsList.Count; i++)
        {
            if (_bulletsList[i] != null)
            {
                Destroy(_bulletsList[i]);
            }
        }

        _bulletsList.Clear();
    }

    private void OnDisable()
    {
        ProjectileMoveScript.DeactivateAllActiveBullets -= DeactivateBullets;
        ShootTrigger.StartShooting -= BeginAttacking;
        _isAttacking = false;
    }
}
