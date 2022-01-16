using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidMover : MonoBehaviour
{
    private Rigidbody rb;

    [SerializeField]
    private float separation = 1;
    [SerializeField]
    private float alignment = 1;
    [SerializeField]
    private float cohesion = 1;


    [SerializeField]
    private float speed = 10;
    [SerializeField]
    private float rotationSpeed = 10;
    [SerializeField]
    private float radius = 3;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    void OnTriggerStay(Collider coll)
    {
        Transform other = coll.gameObject.transform;
        Separation(other);
        Alignment(other);
        Cohesion(other);

        ObstacleAvoidance();
    }

    void Separation(Transform other)
    {
        float distance = (other.position-transform.position).magnitude;
        Vector3 targetPos = transform.InverseTransformPoint(other.position);

        if (distance <= separation)
        {
            if (targetPos.x < 0)
            {
                transform.Rotate(0, rotationSpeed*Time.deltaTime, 0);
            }
            if (targetPos.x > 0)
            {
                transform.Rotate(0, -1*rotationSpeed*Time.deltaTime, 0);
            }
        }
    }

    void Alignment(Transform other)
    {

    }

    void Cohesion(Transform other)
    {

    }

    void ObstacleAvoidance()
    {
        RaycastHit hit;
        Debug.DrawRay(transform.position, transform.forward*radius, Color.green);
        if (Physics.Raycast(transform.position, transform.forward, out hit, radius, Layers.Instance.obstacles))
        {
            //TO DO: make 2 more rays to determine which diirection to go in, left and right
        }
    }
}
