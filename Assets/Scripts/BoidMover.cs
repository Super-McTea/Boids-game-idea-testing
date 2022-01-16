using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidMover : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField]
    private float speed = 10;
    [SerializeField]
    private float rotationSpeed = 10;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(string.Format("Position: {0}", rb.position));
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    void OnTriggerStay(Collider other)
    {
        Separation();
        Alignment();
        Cohesion();
    }

    void Separation()
    {

    }

    void Alignment()
    {

    }

    void Cohesion()
    {

    }
}
