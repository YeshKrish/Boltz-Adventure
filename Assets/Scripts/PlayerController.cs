using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
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

    private Rigidbody _rb;
    private SphereCollider _ballSphereCollider;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _ballSphereCollider = GetComponent<SphereCollider>();
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
            _rb.AddForce(new Vector3(0f, transform.position.y, 0f).normalized * _jumpHeight, ForceMode.Impulse);
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
        Debug.Log(collision.gameObject.layer);
        if((( 1 << collision.gameObject.layer) & _enemyLayer) != 0)
        {
            Debug.Log("Plater Dead");
            GameManager.instance.GameOver();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.tag);
        if (other.gameObject.CompareTag("EnemyHead"))
        {
            other.transform.parent.gameObject.SetActive(false);
        }
    }
}

