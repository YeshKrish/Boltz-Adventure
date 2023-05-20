using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;


[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private GameObject _tower;
    [SerializeField]
    private GameObject _fish;
    [SerializeField]
    private float speed;
    [SerializeField]
    private int _jumpHeight = 6;

    [SerializeField]
    private VariableJoystick variableJoystick;
    
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

    private Vector3 _ballVelocity;
    private Vector3 _initialVelocity;
    public float joystickSensitivity = 2.0f;
    public float maxVelocity = 7.5f;

    private void Start()
    {
        //Get FinalLevel from BuildIndex
        _fifthLevel = SceneUtility.GetScenePathByBuildIndex(5);
        _sixthLevel = SceneUtility.GetScenePathByBuildIndex(6);
        _fifthLevelName = GetLevelName(_fifthLevel);
        _sixthLevelName = GetLevelName(_sixthLevel);

        _rb = GetComponent<Rigidbody>();
        _ballSphereCollider = GetComponent<SphereCollider>();

        _initialVelocity = _rb.velocity;
    }

    private void Update()
    {
        if (Vector3.Distance(transform.position, _tower.transform.position) > _doorToBeOpenedDist && SceneManager.GetActiveScene().name == _fifthLevelName && !GameManager.instance.isDoorOpened)
        {
            UIManager.Instance.QuestTextObj.SetActive(false);
        }
        if(SceneManager.GetActiveScene().name == _sixthLevelName)
        {
            if (Vector3.Distance(transform.position, _fish.transform.position) < _fishImageToBeSpawnedDistance && !SpecialFish._isFishDead)
            {
                UIManager.Instance.FishTextObj.SetActive(true);
            }
            else if (Vector3.Distance(transform.position, _fish.transform.position) > _fishImageToBeSpawnedDistance)
            {
                UIManager.Instance.FishTextObj.SetActive(false);
            }

            if (SpecialFish._isFishDead)
            {
                UIManager.Instance.FishTextObj.SetActive(false);
            }
        }

        if (Vector3.Distance(transform.position, _tower.transform.position) < _doorToBeOpenedDist && SceneManager.GetActiveScene().name != _fifthLevelName)
        {
            DoorOpen?.Invoke();
        }
        else if(Vector3.Distance(transform.position, _tower.transform.position) < _doorToBeOpenedDist && !GameManager.instance.isDoorOpened)
        {
            UIManager.Instance.QuestTextObj.SetActive(true);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }

        OnFalling();

        _ballVelocity = _rb.velocity;
    }

    private void FixedUpdate()
    {
        float moveHorizontal = variableJoystick.Horizontal * joystickSensitivity;
        Vector3 direction = Vector3.right * moveHorizontal;

        _rb.AddForce(direction * speed * Time.fixedDeltaTime, ForceMode.VelocityChange);
        // Limit the velocity of the ball
        if (Mathf.Abs(_rb.velocity.x) > maxVelocity)
        {
            float sign = Mathf.Sign(_rb.velocity.x);
            _rb.velocity = new Vector2(sign * maxVelocity, _rb.velocity.y);
        }
    }

    public void Jump()
    {
        if (IsGrounded())
        {
            MusicManager.instance.JumpSound();
            _rb.AddForce(new Vector3(0f, Math.Abs(transform.position.y), 0f).normalized * _jumpHeight, ForceMode.Impulse);
        }
    }

    private bool IsGrounded(float length = 0.2f)
    {
        if(Physics.SphereCast(_ballSphereCollider.transform.position, _ballSphereCollider.radius/2f, Vector3.down, out RaycastHit hit, _ballSphereCollider.bounds.extents.y + 0.1f, _groundLayer))
        {
            return true;
        }
        return false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if((( 1 << collision.gameObject.layer) & _enemyLayer) != 0)
        {
            KilledByEnemy?.Invoke();
            GameManager.instance.GameOver();
        }
        if (collision.gameObject.CompareTag("Spikes"))
        {
            GameManager.instance.GameOver();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("EnemyHead"))
        {
            other.gameObject.SetActive(false);
            MusicManager.instance.EnemyDyingSound();
            _rb.AddForce(new Vector3(0f, Math.Abs(transform.position.y), 0f).normalized * _enemyDeadJumpHeight, ForceMode.Impulse);
            if (other.transform.parent.gameObject.GetComponentInChildren<MeshCollider>() != null)
            {
                if(other.transform.parent.gameObject.name == "EnemyBody")
                {
                    for (int i = 0; i < 3; i++)
                    {
                        other.transform.parent.gameObject.GetComponentsInChildren<MeshCollider>()[i].enabled = false;
                    }
                }
                else
                {
                    other.transform.parent.gameObject.GetComponentInChildren<MeshCollider>().enabled = false;
                }
            }
            other.GetComponentInParent<WayPointFollower>().enabled = false;
            other.transform.parent.gameObject.transform.parent.GetComponent<Animator>().SetBool("isDead", true);
            Destroy(other.transform.parent.gameObject, 2f);
        }
        if (((1 << other.gameObject.layer) & _winLayer) != 0)
        {
            StartCoroutine(Win());
        } 
        if (other.gameObject.CompareTag("BouncingHead"))
        {
            MusicManager.instance.SpringSound();
            if (_ballVelocity.y < 0f)
            {
                _ballVelocity.y = 0f;
                _rb.velocity = _ballVelocity;
            }
            else { _rb.velocity = _ballVelocity; }
            Bounce?.Invoke();
            //_rb.AddForce(new Vector3(0f, Math.Abs(transform.position.y), 0f).normalized * _bounceHeight, ForceMode.Impulse);
        }
        if (((1 << other.gameObject.layer) & _collectibleLayer) != 0)
        {
            Item hitObject = other.gameObject.GetComponent<Consumables>().item;
            if(hitObject != null)
            {
                MusicManager.instance.CoinCollectSound();
                UIManager.Instance.UpdateScoreText();
                other.gameObject.SetActive(false);
            }
        } 
        if(((1 << other.gameObject.layer) & _waterLayer) != 0)
        {
            GameManager.instance.GameOver();
        }
    }

    IEnumerator Win()
    {
        yield return new WaitForSeconds(.8f);
        LevelCompleted?.Invoke();
    }

    void OnFalling()
    {
        if(!Physics.SphereCast(_ballSphereCollider.transform.position, _ballSphereCollider.radius / 2f, Vector3.down, out RaycastHit hit, _ballSphereCollider.bounds.extents.y + 10f, _groundLayer) )
        {
            if(transform.position.y < -10f)
            GameManager.instance.GameOver();

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

