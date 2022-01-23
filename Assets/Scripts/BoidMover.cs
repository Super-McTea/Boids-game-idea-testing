using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidMover : MonoBehaviour
{
    private SphereCollider sphereCollider;
    private float minColliderRadius = 0;

    [SerializeField]
    private float separationFactor = 1;
    [SerializeField]
    private float alignmentFactor = 1;
    [SerializeField]
    private float cohesionPercentage = 1;


    [SerializeField]
    private float speed = 10;
    [SerializeField]
    private float turningRadius = 5;
    private float angle;

    private float checkAngle = 35; // degrees

    [SerializeField]
    private float jitterFactor = 1;
    [SerializeField]
    private int maxBoidChecks = 5;


    private List<Transform> closeBoids = new List<Transform>();

    private bool isObstacleAvoiding = false;

    private int boidFrameCounter = 0;


    // Start is called before the first frame update
    void Start()
    {
        sphereCollider = GetComponent<SphereCollider>();
        angle = (360*speed)/(6*turningRadius);
        //turningRadius = (360*speed)/(6.2f*angle);
        minColliderRadius = separationFactor+1;
        sphereCollider.radius = minColliderRadius + turningRadius;
    }

    // Update is called once per frame
    void Update()
    {
        if (closeBoids.Count > maxBoidChecks && sphereCollider.radius >= minColliderRadius)
        {
            sphereCollider.radius -= 0.5f;
        }
        else if (closeBoids.Count < maxBoidChecks && sphereCollider.radius <= minColliderRadius*3)
        {
            sphereCollider.radius += 0.5f;
        }
    }

    void FixedUpdate()
    {
        boidFrameCounter = 0;
        
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
        transform.Rotate(0, Random.Range(-1.0f,1.0f)*angle*jitterFactor*Time.deltaTime, 0);
        if (isObstacleAvoiding)
        {
            Debug.DrawRay(transform.position, transform.forward*turningRadius, Color.blue);
        }
        else
        {
            Debug.DrawRay(transform.position, transform.forward*turningRadius, Color.green);
        }
    }

    void OnTriggerStay(Collider col)
    {
        if (Layers.Instance.obstacles.Contains(col.gameObject))
        {
            isObstacleAvoiding = true;
            sphereCollider.radius = minColliderRadius;
            ObstacleAvoidance();
        }
        else if (!isObstacleAvoiding)
        {
            if (Layers.Instance.boids.Contains(col.gameObject) && boidFrameCounter <= maxBoidChecks)
            {
                boidFrameCounter += 1;
                Transform other = col.gameObject.transform;
                Separation(other);
                Alignment(other);
                Cohesion();
            }
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
        if (Layers.Instance.obstacles.Contains(other))
        {
            isObstacleAvoiding = false;
        }
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

    void Alignment(Transform other)
    {
        // Boids have to set their rotation to the average of everyone elses.
        float targetRotationY = wrapAround(other.eulerAngles.y - transform.eulerAngles.y);

        if (targetRotationY > angle)
        {
            targetRotationY = angle;
        }
        else if (targetRotationY < -angle)
        {
            targetRotationY = -angle;
        }

        Debug.DrawRay(transform.position, Quaternion.Euler(0,targetRotationY,0)*transform.forward*turningRadius, Color.red);
        transform.Rotate(0, targetRotationY*alignmentFactor*Time.deltaTime, 0);
    }

    void Cohesion()
    {
        // Boids move towards the centre of everyone else.
    }

    void ObstacleAvoidance()
    {
        RaycastHit front;
        bool frontRay = Physics.Raycast(transform.position, transform.forward, out front, turningRadius, Layers.Instance.obstacles);

        RaycastHit left;
        Vector3 leftAngleFront = Quaternion.Euler(0,-checkAngle,0) * transform.forward;
        RaycastHit leftMid;
        Vector3 leftAngleMid = Quaternion.Euler(0,-checkAngle*2,0) * transform.forward;
        RaycastHit leftBack;
        Vector3 leftAngleBack = Quaternion.Euler(0,-checkAngle*3,0) * transform.forward;

        RaycastHit right;
        Vector3 rightAngleFront = Quaternion.Euler(0,checkAngle,0) * transform.forward;
        RaycastHit rightMid;
        Vector3 rightAngleMid = Quaternion.Euler(0,checkAngle*2,0) * transform.forward;
        RaycastHit rightBack;
        Vector3 rightAngleBack = Quaternion.Euler(0,checkAngle*3,0) * transform.forward;
        
        bool leftFrontRay = Physics.Raycast(transform.position, leftAngleFront, out left, turningRadius*2, Layers.Instance.obstacles);
        bool leftMidRay = Physics.Raycast(transform.position, leftAngleMid, out leftMid, turningRadius*2, Layers.Instance.obstacles);
        bool leftBackRay = Physics.Raycast(transform.position, leftAngleBack, out leftBack, turningRadius*2, Layers.Instance.obstacles);
        bool leftRay = leftFrontRay || leftMidRay || leftBackRay;

        bool rightFrontRay = Physics.Raycast(transform.position, rightAngleFront, out right, turningRadius*2, Layers.Instance.obstacles);
        bool rightMidRay = Physics.Raycast(transform.position, rightAngleMid, out rightMid, turningRadius*2, Layers.Instance.obstacles);
        bool rightBackRay = Physics.Raycast(transform.position, rightAngleBack, out rightBack, turningRadius*2, Layers.Instance.obstacles);
        bool rightRay = rightFrontRay || rightMidRay || rightBackRay;

        Debug.DrawRay(transform.position, leftAngleFront*turningRadius*2, Color.blue);
        Debug.DrawRay(transform.position, leftAngleMid*turningRadius*2, Color.blue);
        Debug.DrawRay(transform.position, leftAngleBack*turningRadius*2, Color.blue);

        Debug.DrawRay(transform.position, rightAngleFront*turningRadius*2, Color.blue);
        Debug.DrawRay(transform.position, rightAngleMid*turningRadius*2, Color.blue);
        Debug.DrawRay(transform.position, rightAngleBack*turningRadius*2, Color.blue);
        
        float leftDistance = left.distance;
        if (leftMid.distance < leftDistance)
        {
            leftDistance = leftMid.distance;
        }
        if (leftBack.distance < leftDistance)
        {
            leftDistance = leftBack.distance;
        }

        float rightDistance = right.distance;
        if (rightMid.distance < rightDistance)
        {
            rightDistance = rightMid.distance;
        }
        if (rightBack.distance < rightDistance)
        {
            rightDistance = rightBack.distance;
        }

        if (frontRay)
        {
            if (leftRay)
            {
                TurnRight(leftDistance);
            }
            else if (rightRay)
            {
                TurnLeft(rightDistance);
            }
            else
            {
                TurnLeft(rightDistance);
            }
        }
        else
        {
            if (leftRay && rightRay)
            {
                TurnLeft(rightDistance);
            }
            else if (rightRay)
            {
                TurnLeft(rightDistance);
            }
            else if (leftRay)
            {
                TurnRight(leftDistance);
            }
            else
            {
                isObstacleAvoiding = false;
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

    float wrapAround(float boundedAngle)
    {
        while (boundedAngle > 180)
        {
            boundedAngle -= 360;
        }
        while (boundedAngle <= -180)
        {
            boundedAngle += 360;
        }
        return boundedAngle;
    }

}
