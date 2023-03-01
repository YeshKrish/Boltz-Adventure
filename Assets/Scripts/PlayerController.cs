using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float speed;
    [SerializeField]
    private VariableJoystick variableJoystick;
    [SerializeField]
    private Rigidbody rb;

    public void FixedUpdate()
    {
        Vector3 direction = Vector3.right * variableJoystick.Horizontal;
        rb.AddForce(direction * speed * Time.fixedDeltaTime, ForceMode.VelocityChange);
    }

}

