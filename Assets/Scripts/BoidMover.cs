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
    private float separationDistance = 1;
    [SerializeField]
    private float separationStrength = 10;
    [SerializeField]
    private float alignmentFactor = 1;
    [SerializeField]
    private float cohesionFactor = 1;
    [SerializeField]
    private float avoidanceFactor = 2;

    private Vector3 targetVector;


    [SerializeField]
    private float speed = 10;
    [SerializeField]
    private float rayLength = 5;
    private float angle;

    private int rayCount = 30;       // per side

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
        minColliderRadius = separationDistance+1;
        sphereCollider.radius = minColliderRadius + rayLength;

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
        Separation();
        ObstacleAvoidance();

        float targetAngle = Vector3.SignedAngle(transform.forward, targetVector, Vector3.up);

        Debug.DrawRay(transform.position, Quaternion.Euler(0, targetAngle, 0)*transform.forward*3, Color.yellow);

        transform.Rotate(0, targetAngle, 0);
        transform.Translate(Vector3.forward * Vector3.ClampMagnitude(targetVector, speed).magnitude * Time.deltaTime);

        targetVector = transform.forward;

        // if (isObstacleAvoiding)
        // {
        //     Debug.DrawRay(transform.position, transform.forward*separationDistance, Color.blue);
        // }
        // else
        // {
        //     Debug.DrawRay(transform.position, transform.forward*separationDistance, Color.green);
        // }
    }

    void OnTriggerStay(Collider col)
    {
        if (Layers.Instance.boids.Contains(col.gameObject) && boidFrameCounter <= maxBoidChecks)
        {
            boidFrameCounter += 1;
            Transform other = col.gameObject.transform;
            

            BoidMover otherBoidMover = col.gameObject.GetComponent<BoidMover>();
            if (otherBoidMover.BoidFlock == boidFlock && isInFOV(other))
            {
                Alignment(other);
                Cohesion(other);
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

    void Separation()
    {
        Vector3 separationVector = Vector3.zero;
        for (int i = 0; i < closeBoids.Count; i++)
        {
            float distance = (closeBoids[i].position-transform.position).magnitude;
            float targetPosX = transform.InverseTransformPoint(closeBoids[i].position).x;

            if (distance <= separationDistance)
            {
                Vector3 direction = transform.position-closeBoids[i].position;

                Debug.DrawRay(transform.position, (direction)*(separationDistance-distance), Color.red);
                
                separationVector += direction.normalized*(separationDistance-distance);
            }
        }
        Debug.DrawRay(transform.position, separationVector, Color.blue);
        targetVector = targetVector + separationVector*separationStrength;
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

        transform.Rotate(0, targetRotationY*alignmentFactor*Time.deltaTime, 0);
    }

    void Cohesion(Transform other)
    {
        // Boids move towards the centre of everyone else.
        float distance = (other.position-transform.position).magnitude;

        if (distance > separationDistance)
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

            transform.Rotate(0, targetAngle*cohesionFactor*Time.deltaTime, 0);
        }
    }

    void ObstacleAvoidance()
    {
        RaycastHit rayHit;
        Vector3 rayAngle = Vector3.zero;
        bool didRayHit = false;

        for (int i = 0; i < rayCount*2; i++)
        {
            rayAngle = Quaternion.Euler(0, Mathf.Pow(-1,i)*(fieldOfViewAngle/(rayCount))*((i+1)/2), 0) * transform.forward;
            Debug.DrawRay(transform.position, rayAngle*rayLength, Color.blue);
            
            didRayHit = Physics.Raycast(transform.position, rayAngle, out rayHit, rayLength, Layers.Instance.obstacles);
            
            if (!didRayHit)
            {
                Debug.DrawRay(transform.position, rayAngle*rayLength, Color.green);
                targetVector = targetVector + rayAngle.normalized*avoidanceFactor*10;
                return;
            }
        }
        targetVector = targetVector + rayAngle.normalized*avoidanceFactor*10;
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
