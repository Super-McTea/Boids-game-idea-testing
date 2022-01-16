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
    private float turningRadius = 5;
    private float angle;

    private float checkAngle = 30; // degrees

    [SerializeField]
    private float jitterFactor = 1;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        angle = (360*speed)/(3*turningRadius);
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
        transform.Rotate(0, Random.Range(-1.0f,1.0f)*angle*jitterFactor*Time.deltaTime, 0);
    }

    void OnTriggerStay(Collider coll)
    {
        if (Layers.Instance.obstacles.Contains(coll.gameObject))
        {
            ObstacleAvoidance();
        }
        else
        {
            Transform other = coll.gameObject.transform;
            Separation(other);
            Alignment(other);
            Cohesion(other);
        }
    }

    void Separation(Transform other)
    {
        float distance = (other.position-transform.position).magnitude;
        Vector3 targetPos = transform.InverseTransformPoint(other.position);

        if (distance <= separation)
        {
            if (targetPos.x < 0)
            {
                TurnRight();
            }
            if (targetPos.x > 0)
            {
                TurnLeft();
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
        RaycastHit left;
        Vector3 leftAngle = Quaternion.Euler(0,-checkAngle,0) * transform.forward;
        RaycastHit right;
        Vector3 rightAngle = Quaternion.Euler(0,checkAngle,0) * transform.forward;
        
        bool frontRay = Physics.Raycast(transform.position, transform.forward, out hit, turningRadius, Layers.Instance.obstacles);
        bool leftRay = Physics.Raycast(transform.position, leftAngle, out left, turningRadius*2, Layers.Instance.obstacles);
        bool rightRay = Physics.Raycast(transform.position, rightAngle, out right, turningRadius*2, Layers.Instance.obstacles);

        Debug.DrawRay(transform.position, transform.forward*turningRadius, Color.green);
        Debug.DrawRay(transform.position, leftAngle*turningRadius*2, Color.blue);
        Debug.DrawRay(transform.position, rightAngle*turningRadius*2, Color.blue);
        
        if (frontRay)
        {
            if (leftRay)
            {
                TurnRight();
            }
            else if (rightRay)
            {
                TurnLeft();
            }
            else
            {
                TurnRight();
            }
        }
        else
        {
            if (leftRay && rightRay)
            {
                TurnLeft();
            }
            else if (rightRay)
            {
                TurnLeft();
            }
            else if (leftRay)
            {
                TurnRight();
            }
        }
    }

    void TurnLeft()
    {
        transform.Rotate(0, -1*angle*Time.deltaTime, 0);
    }
    void TurnRight()
    {
        transform.Rotate(0, angle*Time.deltaTime, 0);
    }

}
