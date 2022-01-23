using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    private float speed = 5;
    [SerializeField]
    private float rotationSpeed = 5;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    void FixedUpdate()
    {
        float vertAxis = Input.GetAxis("Vertical");
        float horiAxis = Input.GetAxis("Horizontal");

        rb.MovePosition(rb.position + transform.forward*speed*vertAxis*Time.fixedDeltaTime);
        rb.MoveRotation(rb.rotation * Quaternion.Euler(new Vector3(0,rotationSpeed,0) * Time.fixedDeltaTime * horiAxis));
    }
}
