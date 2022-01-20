using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidMover : MonoBehaviour
{
    private Rigidbody rb;

    [SerializeField]
    private float separationFactor = 1;
    [SerializeField]
    private float alignmentPercentage = 1;
    [SerializeField]
    private float cohesionPercentage = 1;


    [SerializeField]
    private float speed = 10;
    [SerializeField]
    private float turningRadius = 5;
    private float angle;

    private float checkAngle = 30; // degrees

    [SerializeField]
    private float jitterFactor = 1;


    private List<Transform> closeBoids = new List<Transform>();


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

    void OnTriggerStay(Collider col)
    {
        if (Layers.Instance.obstacles.Contains(col.gameObject))
        {
            ObstacleAvoidance();
        }
        else
        {
            Transform other = col.gameObject.transform;
            Separation(other);
            Alignment();
            Cohesion();
        }
    }

    void OnTriggerEnter(Collider col)
    {
        GameObject other = col.gameObject;
        if (Layers.Instance.boids.Contains(other))
        {
            if (!closeBoids.Contains(other.transform))
            {
                closeBoids.Add(other.transform);
            }
        }
    }
    void OnTriggerExit(Collider col)
    {
        GameObject other = col.gameObject;
        if (Layers.Instance.boids.Contains(other))
        {
            closeBoids.Remove(other.transform);
        }
    }

    void Separation(Transform other)
    {
        float distance = (other.position-transform.position).magnitude;
        Vector3 targetPos = transform.InverseTransformPoint(other.position);

        if (distance <= separationFactor)
        {
            if (targetPos.x < 0)
            {
                TurnRight(turningRadius*2);
            }
            if (targetPos.x > 0)
            {
                TurnLeft(turningRadius*2);
            }
        }
    }

    void Alignment()
    {
        // Boids have to set their rotation to the average of everyone elses.
    }

    void Cohesion()
    {
        // Boids move towards the centre of everyone else.
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
                TurnRight(left.distance);
            }
            else if (rightRay)
            {
                TurnLeft(right.distance);
            }
            else
            {
                TurnRight(left.distance);
            }
        }
        else
        {
            if (leftRay && rightRay)
            {
                TurnLeft(right.distance);
            }
            else if (rightRay)
            {
                TurnLeft(right.distance);
            }
            else if (leftRay)
            {
                TurnRight(left.distance);
            }
        }
    }

    void TurnLeft(float rayLength)
    {
        transform.Rotate(0, -1*angle*Time.deltaTime, 0);
        if (rayLength <= turningRadius)
        {
            transform.Rotate(0, -1*angle*Time.deltaTime, 0);
        }
    }
    void TurnRight(float rayLength)
    {
        transform.Rotate(0, angle*Time.deltaTime, 0);
        if (rayLength <= turningRadius)
        {
            transform.Rotate(0, angle*Time.deltaTime, 0);
        }
    }

}
