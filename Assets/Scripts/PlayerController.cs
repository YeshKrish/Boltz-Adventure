using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;


[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private GameObject _tower;
    [SerializeField]
    private GameObject _fish;    
    [SerializeField]
    [FormerlySerializedAs("speed")]
    [Tooltip("How hard the joystick pushes the ball along the x axis.")]
    private float _speed;
    [SerializeField]
    private int _jumpHeight = 6;

    [SerializeField]
    [FormerlySerializedAs("variableJoystick")]
    private VariableJoystick _variableJoystick;
    
    [SerializeField]
    private LayerMask _groundLayer;  
    [SerializeField]
    private LayerMask _enemyLayer;
    [SerializeField]
    private LayerMask _winLayer;
    [SerializeField]
    private LayerMask _bouncingLayer;    
    [SerializeField]
    private LayerMask _collectibleLayer;  
    [SerializeField]
    private LayerMask _waterLayer;

    private Rigidbody _rb;
    private SphereCollider _ballSphereCollider;

    private int _doorToBeOpenedDist = 10;
    private int _enemyDeadJumpHeight = 4;
    private int _fishImageToBeSpawnedDistance = 8;

    //Special Levels
    private string _fifthLevel;
    private string _sixthLevel;
    private string _fifthLevelName;
    private string _sixthLevelName;

    public static event Action DoorOpen;
    public static event Action LevelCompleted;
    public static event Action Bounce;
    public static event Action KilledByEnemy;
    public static event Action ActivateFightCamera;
    public static event Action DeActivateFightCamera;

    private Vector3 _ballVelocity;
    [SerializeField]
    [FormerlySerializedAs("joystickSensitivity")]
    [Tooltip("Multiplier on the joystick's horizontal reading before it becomes force.")]
    private float _joystickSensitivity = 2.0f;

    [SerializeField]
    [FormerlySerializedAs("maxVelocity")]
    [Tooltip("Fastest the ball may travel along the x axis.")]
    private float _maxVelocity = 7.5f;

    private void Start()
    {
        //Get FinalLevel from BuildIndex
        _fifthLevel = SceneUtility.GetScenePathByBuildIndex(5);
        _sixthLevel = SceneUtility.GetScenePathByBuildIndex(6);
        _fifthLevelName = GetLevelName(_fifthLevel);
        _sixthLevelName = GetLevelName(_sixthLevel);

        _rb = GetComponent<Rigidbody>();
        _ballSphereCollider = GetComponent<SphereCollider>();
    }

    private void Update()
    {
        if (Vector3.Distance(transform.position, _tower.transform.position) > _doorToBeOpenedDist && SceneManager.GetActiveScene().name == _fifthLevelName && !GameManager.Instance.IsDoorOpened)
        {
            UIManager.Instance.QuestTextObj.SetActive(false);
        }
        if(SceneManager.GetActiveScene().name == _sixthLevelName)
        {
            //SecondCameraTrigger
            if(!SpecialMonsters.IsAlienDead && ShootTrigger.IsPlayerInShootingArea)
            {
                ActivateFightCamera?.Invoke();
            }
            else if (SpecialMonsters.IsAlienDead)
            {
                DeActivateFightCamera?.Invoke();
            }

            if (Vector3.Distance(transform.position, _fish.transform.position) < _fishImageToBeSpawnedDistance && !SpecialFish.IsFishDead)
            {
                UIManager.Instance.FishTextObj.SetActive(true);
            }
            else if (Vector3.Distance(transform.position, _fish.transform.position) > _fishImageToBeSpawnedDistance)
            {
                UIManager.Instance.FishTextObj.SetActive(false);
            }

            if (SpecialFish.IsFishDead)
            {
                UIManager.Instance.FishTextObj.SetActive(false);
            }
        }

        if (Vector3.Distance(transform.position, _tower.transform.position) < _doorToBeOpenedDist && SceneManager.GetActiveScene().name != _fifthLevelName)
        {
            DoorOpen?.Invoke();
        }
        else if(Vector3.Distance(transform.position, _tower.transform.position) < _doorToBeOpenedDist && !GameManager.Instance.IsDoorOpened)
        {
            UIManager.Instance.QuestTextObj.SetActive(true);
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }
#endif
    }

    private void FixedUpdate()
    {
        float moveHorizontal = _variableJoystick.Horizontal * _joystickSensitivity;
        Vector3 direction = Vector3.right * moveHorizontal;

        _rb.AddForce(direction * _speed * Time.fixedDeltaTime, ForceMode.VelocityChange);

        // Clamping used to assign a Vector2 to Rigidbody.linearVelocity, which converts with
        // z = 0 and silently killed any depth movement the ball had picked up.
        if (Mathf.Abs(_rb.linearVelocity.x) > _maxVelocity)
        {
            Vector3 velocity = _rb.linearVelocity;
            velocity.x = Mathf.Sign(velocity.x) * _maxVelocity;
            _rb.linearVelocity = velocity;
        }

        _ballVelocity = _rb.linearVelocity;

        OnFalling();
    }

    public void Jump()
    {
        if (IsGrounded())
        {
            MusicManager.Instance.JumpSound();
            _rb.AddForce(new Vector3(0f, Math.Abs(transform.position.y), 0f).normalized * _jumpHeight, ForceMode.Impulse);
        }
    }

    private bool IsGrounded()
    {
        return Physics.SphereCast(
            _ballSphereCollider.transform.position,
            _ballSphereCollider.radius / 2f,
            Vector3.down,
            out _,
            _ballSphereCollider.bounds.extents.y + 0.1f,
            _groundLayer);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if((( 1 << collision.gameObject.layer) & _enemyLayer) != 0)
        {
            KilledByEnemy?.Invoke();
            GameManager.Instance.GameOver();
        }
        if (collision.gameObject.CompareTag("Spikes"))
        {
            GameManager.Instance.GameOver();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("EnemyHead"))
        {
            Enemy enemy = other.GetComponentInParent<Enemy>();
            if (enemy != null)
            {
                MusicManager.Instance.EnemyDyingSound();
                _rb.AddForce(new Vector3(0f, Math.Abs(transform.position.y), 0f).normalized * _enemyDeadJumpHeight, ForceMode.Impulse);
                enemy.Kill();
            }
        }
        if (((1 << other.gameObject.layer) & _winLayer) != 0)
        {
            StartCoroutine(Win());
        } 
        if (other.gameObject.CompareTag("BouncingHead"))
        {
            MusicManager.Instance.SpringSound();
            if (_ballVelocity.y < 0f)
            {
                _ballVelocity.y = 0f;
                _rb.linearVelocity = _ballVelocity;
            }
            else { _rb.linearVelocity = _ballVelocity; }
            Bounce?.Invoke();
            //_rb.AddForce(new Vector3(0f, Math.Abs(transform.position.y), 0f).normalized * _bounceHeight, ForceMode.Impulse);
        }
        if (((1 << other.gameObject.layer) & _collectibleLayer) != 0)
        {
            Item hitObject = other.gameObject.GetComponent<Consumables>().item;
            if(hitObject != null)
            {
                MusicManager.Instance.CoinCollectSound();
                UIManager.Instance.UpdateScoreText();
                other.gameObject.SetActive(false);
            }
        } 
        if(((1 << other.gameObject.layer) & _waterLayer) != 0)
        {
            GameManager.Instance.GameOver();
        }
    }

    IEnumerator Win()
    {
        yield return new WaitForSeconds(.8f);
        LevelCompleted?.Invoke();
    }

    private void OnFalling()
    {
        bool groundWithinReach = Physics.SphereCast(
            _ballSphereCollider.transform.position,
            _ballSphereCollider.radius / 2f,
            Vector3.down,
            out _,
            _ballSphereCollider.bounds.extents.y + 10f,
            _groundLayer);

        if (!groundWithinReach && transform.position.y < -10f)
        {
            GameManager.Instance.GameOver();
        }
    }

    private string GetLevelName(string level)
    {
        int slash = level.LastIndexOf('/');
        string name = level.Substring(slash + 1);
        int dot = name.LastIndexOf('.');
        string levelName = name.Substring(0, dot);
        return levelName;
    }
}

