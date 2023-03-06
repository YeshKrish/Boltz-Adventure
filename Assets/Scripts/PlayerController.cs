using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;


[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private GameObject _tower;
    [SerializeField]
    private float speed;
    [SerializeField]
    private int _jumpHeight = 6;
    [SerializeField]
    private int _bounceHeight;

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

    private Rigidbody _rb;
    private SphereCollider _ballSphereCollider;
    private Bouncer _bouncer;

    private int _doorToBeOpenedDist = 10;

    public static event Action DoorOpen;
    public static event Action LevelCompleted;
    public static event Action Bounce;
    //public static event Action EnemyDead;

    private void Start()
    {
        Debug.Log(_bounceHeight);
        _rb = GetComponent<Rigidbody>();
        _ballSphereCollider = GetComponent<SphereCollider>();
    }

    private void Update()
    {
        if (Vector3.Distance(transform.position, _tower.transform.position) > _doorToBeOpenedDist && SceneManager.GetActiveScene().name == "Level5")
        {
            UIManager.Instance.QuestTextObj.SetActive(false);
        }

        if (Vector3.Distance(transform.position, _tower.transform.position) < _doorToBeOpenedDist && SceneManager.GetActiveScene().name != "Level5")
        {
            DoorOpen?.Invoke();
        }
        else if(Vector3.Distance(transform.position, _tower.transform.position) < _doorToBeOpenedDist)
        {
            UIManager.Instance.QuestTextObj.SetActive(true);
        }

        OnFalling();
    }

    private void FixedUpdate()
    {
        Vector3 direction = Vector3.right * variableJoystick.Horizontal;
        _rb.AddForce(direction * speed * Time.fixedDeltaTime, ForceMode.VelocityChange);
    }

    public void Jump()
    {
        //Debug.Log(IsGrounded());
        if (IsGrounded())
        {
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
            Debug.Log("Plater Dead");
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
            //Debug.Log(other.transform.parent.gameObject.GetComponentInChildren<MeshCollider>());
            if (other.transform.parent.gameObject.GetComponentInChildren<MeshCollider>() != null)
            {
                other.transform.parent.gameObject.GetComponentInChildren<MeshCollider>().enabled = false;
            }
            other.GetComponentInParent<WayPointFollower>().enabled = false;
            other.transform.parent.gameObject.transform.parent.GetComponent<Animator>().SetBool("isDead", true);
            Destroy(other.transform.parent.gameObject, 2f);
            //EnemyDead?.BeginInvoke(other);
        }
        if (((1 << other.gameObject.layer) & _winLayer) != 0)
        {
            StartCoroutine(Win());
        } 
        if (other.gameObject.CompareTag("BouncingHead"))
        {
            Bounce?.Invoke();
            _rb.AddForce(new Vector3(0f, Math.Abs(transform.position.y), 0f).normalized * _bounceHeight, ForceMode.Impulse);
        }
        if (((1 << other.gameObject.layer) & _collectibleLayer) != 0)
        {
            Item hitObject = other.gameObject.GetComponent<Consumables>().item;
            if(hitObject != null)
            {
                UIManager.Instance.UpdateScoreText();
                other.gameObject.SetActive(false);
            }
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

    //IEnumerator WaitForFewSeconds()
    //{
    //    yield return new WaitForSeconds(.5f);
    //    if (!Physics.SphereCast(_ballSphereCollider.transform.position, _ballSphereCollider.radius / 2f, Vector3.down, out RaycastHit hit, _ballSphereCollider.bounds.extents.y + 10f, _groundLayer))
    //    {
           
    //    }
    //}

}

