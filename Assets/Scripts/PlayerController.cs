using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;



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
    private int _bounceHeight = 11;

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

    private Rigidbody _rb;
    private SphereCollider _ballSphereCollider;

    private int _doorToBeOpenedDist = 10;

    public static event Action DoorOpen;
    public static event Action LevelCompleted;
    public static event Action Bounce;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _ballSphereCollider = GetComponent<SphereCollider>();
    }

    private void Update()
    {
        if (Vector3.Distance(transform.position, _tower.transform.position) < _doorToBeOpenedDist)
        {
            DoorOpen?.Invoke();
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
            other.transform.parent.gameObject.SetActive(false);
        }
        if (((1 << other.gameObject.layer) & _winLayer) != 0)
        {
            Debug.Log("Won");
            LevelCompleted?.Invoke();
        } 
        if (other.gameObject.CompareTag("BouncingHead"))
        {
            Debug.Log("Bounceeeee!");
            Bounce?.Invoke();
            _rb.AddForce(new Vector3(0f, Math.Abs(transform.position.y), 0f).normalized * _bounceHeight, ForceMode.Impulse);
        }
    }

    void OnFalling()
    {
        if(!Physics.SphereCast(_ballSphereCollider.transform.position, _ballSphereCollider.radius / 2f, Vector3.down, out RaycastHit hit, _ballSphereCollider.bounds.extents.y + 10f, _groundLayer))
        {
            StartCoroutine(WaitForFewSeconds());
            
        }
    }

    IEnumerator WaitForFewSeconds()
    {
        yield return new WaitForSeconds(1f);
        if (!Physics.SphereCast(_ballSphereCollider.transform.position, _ballSphereCollider.radius / 2f, Vector3.down, out RaycastHit hit, _ballSphereCollider.bounds.extents.y + 10f, _groundLayer))
        {
            GameManager.instance.GameOver();
        }
    }

}

