using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidMover : MonoBehaviour
{
    private SphereCollider sphereCollider;
    private float minColliderRadius = 0;

    private Renderer boidRenderer;
    private Renderer boidTailRenderer;

    [SerializeField]
    private Color[] colours = new Color[4];

    [SerializeField]
    private float separationFactor = 1;
    [SerializeField]
    private float alignmentFactor = 1;
    [SerializeField]
    private float cohesionFactor = 1;


    [SerializeField]
    private float speed = 10;
    [SerializeField]
    private float turningRadius = 5;
    private float angle;

    private int rayCount = 10;       // per side

    [SerializeField]
    private float jitterFactor = 1;
    [SerializeField]
    private int maxBoidChecks = 5;
    [SerializeField]
    private float fieldOfViewAngle = 90;


    private List<Transform> closeBoids = new List<Transform>();

    private bool isObstacleAvoiding = false;

    private int boidFrameCounter = 0;

    private int boidFlock;
    public int BoidFlock 
    {
        get
        {
            return boidFlock;
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        sphereCollider = GetComponent<SphereCollider>();
        angle = (360*speed)/(6*turningRadius);
        //turningRadius = (360*speed)/(6.2f*angle);
        minColliderRadius = separationFactor+1;
        sphereCollider.radius = minColliderRadius + turningRadius;

        boidTailRenderer = GetComponent<Renderer>();
        boidRenderer = transform.GetChild(0).GetComponent<Renderer>();

        boidFlock = Random.Range(0,colours.Length);
        boidRenderer.material.color = colours[boidFlock];
        boidTailRenderer.material.color = colours[boidFlock];
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
        if (Layers.Instance.obstacles.Contains(col.gameObject) || Layers.Instance.players.Contains(col.gameObject))
        {
            isObstacleAvoiding = true;
            sphereCollider.radius = minColliderRadius;
            ObstacleAvoidance(Layers.Instance.players);
            ObstacleAvoidance(Layers.Instance.obstacles);
        }
        else if (!isObstacleAvoiding)
        {
            if (Layers.Instance.boids.Contains(col.gameObject) && boidFrameCounter <= maxBoidChecks)
            {
                boidFrameCounter += 1;
                Transform other = col.gameObject.transform;
                Separation(other);

                BoidMover otherBoidMover = col.gameObject.GetComponent<BoidMover>();
                if (otherBoidMover.BoidFlock == boidFlock && isInFOV(other))
                {
                    Alignment(other);
                    Cohesion(other);
                }
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
        if (Layers.Instance.obstacles.Contains(other) || Layers.Instance.players.Contains(other))
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

    void Cohesion(Transform other)
    {
        // Boids move towards the centre of everyone else.
        float distance = (other.position-transform.position).magnitude;

        if (distance > separationFactor)
        {
            float targetX = (transform.InverseTransformPoint(other.position).x);
            float targetAngle = Vector3.Angle(other.position-transform.position, transform.forward);
            if (targetX < 0)
            {
                targetAngle = -targetAngle;
            }

            if (Layers.Instance.players.Contains(other.gameObject))
            {
                targetAngle = -targetAngle;
            }

            if (targetAngle > angle)
            {
                targetAngle = angle;
            }
            else if (targetAngle < -angle)
            {
                targetAngle = -angle;
            }

            Debug.DrawRay(transform.position, Quaternion.Euler(0,targetAngle,0)*transform.forward*turningRadius, Color.yellow);
            transform.Rotate(0, targetAngle*cohesionFactor*Time.deltaTime, 0);
        }
    }

    void ObstacleAvoidance(LayerMask layers)
    {
        RaycastHit front;
        bool frontRay = Physics.Raycast(transform.position, transform.forward, out front, turningRadius, Layers.Instance.obstacles);

        RaycastHit[] left = new RaycastHit[rayCount];
        Vector3[] leftAngle = new Vector3[rayCount];

        RaycastHit[] right = new RaycastHit[rayCount];
        Vector3[] rightAngle = new Vector3[rayCount];
        
        bool[] leftRays = new bool[rayCount];
        bool[] rightRays = new bool[rayCount];

        bool leftRay = false;
        bool rightRay = false;

        float leftDistance = float.MaxValue;
        float rightDistance = float.MaxValue;

        for (int i = 0; i < left.Length; i++)
        {
            leftAngle[i] = Quaternion.Euler(0,-(fieldOfViewAngle/(2*rayCount))*(i+1),0) * transform.forward;
            rightAngle[i] = Quaternion.Euler(0,(fieldOfViewAngle/(2*rayCount))*(i+1),0) * transform.forward;

            leftRays[i] = Physics.Raycast(transform.position, leftAngle[i], out left[i], turningRadius*2, layers);
            rightRays[i] = Physics.Raycast(transform.position, rightAngle[i], out right[i], turningRadius*2, layers);

            if (leftRays[i])
            {
                Debug.DrawRay(transform.position, leftAngle[i]*left[i].distance, Color.blue);
            }
            if (rightRays[i])
            {
                Debug.DrawRay(transform.position, rightAngle[i]*right[i].distance, Color.blue);
            }

            if (left[i].distance < leftDistance)
            {
                leftDistance = left[i].distance;
            }
            if (right[i].distance < rightDistance)
            {
                rightDistance = right[i].distance;
            }

            leftRay = leftRay || leftRays[i];
            rightRay = rightRay || rightRays[i];
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

    bool isInFOV(Transform other)
    {
        float targetAngle = Vector3.Angle(other.position-transform.position, transform.forward);
        return (targetAngle <= fieldOfViewAngle);
    }
}
