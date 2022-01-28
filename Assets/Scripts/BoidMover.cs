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
    [SerializeField]
    private float angle = 30;

    private int rayCount = 30;       // per side

    [SerializeField]
    private int maxBoidChecks = 5;
    [SerializeField]
    private float fieldOfViewAngle = 90;
    [SerializeField]
    private float rayFOV = 135;


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
        Alignment();
        if (isObstacleAvoiding)
        {
            ObstacleAvoidance();
        }

        float targetAngle = Vector3.SignedAngle(transform.forward, targetVector, Vector3.up);

        Debug.DrawRay(transform.position, Vector3.ClampMagnitude(targetVector,5), Color.yellow);

        if (targetAngle > angle)
        {
            targetAngle = angle;
        }
        else if (targetAngle < -angle)
        {
            targetAngle = -angle;
        }
        transform.Rotate(0, targetAngle, 0);
        transform.Translate(Vector3.forward * Clamp(targetVector, speed, 1).magnitude * Time.deltaTime);

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
            if (otherBoidMover.BoidFlock == boidFlock && IsInFOV(other))
            {
                
                Cohesion(other);
            }
        }
        if (Layers.Instance.obstacles.Contains(col.gameObject))
        {
            isObstacleAvoiding = true;
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

            if (distance <= separationDistance && IsInFOV(closeBoids[i]))
            {
                Vector3 direction = transform.position-closeBoids[i].position;

                Debug.DrawRay(transform.position, (direction)*(separationDistance-distance), Color.red);
                
                separationVector += direction.normalized*(separationDistance-distance);
            }
        }
        Debug.DrawRay(transform.position, separationVector, Color.blue);
        targetVector = targetVector + separationVector*separationStrength;
    }

    void Alignment()
    {
        Vector3 alignmentVector = Vector3.zero;
        for (int i = 0; i < closeBoids.Count; i++)
        {
            BoidMover boidMover = closeBoids[i].gameObject.GetComponent<BoidMover>();
            if (boidMover.BoidFlock == boidFlock && IsInFOV(closeBoids[i]))
            {
                alignmentVector += closeBoids[i].forward;
            }
        }
        alignmentVector.Normalize();
        Debug.DrawRay(transform.position, alignmentVector*alignmentFactor, Color.green);

        targetVector += alignmentVector*alignmentFactor;
    }

    void Cohesion(Transform other)
    {
        // Boids move towards the centre of everyone else.
        Vector3 cohesionVector = Vector3.zero;
        List<Vector3> cohesiveBoids = new List<Vector3>();

        for (int i = 0; i < closeBoids.Count; i++)
        {
            BoidMover boidMover = closeBoids[i].gameObject.GetComponent<BoidMover>();
            if (boidMover.BoidFlock == boidFlock && IsInFOV(closeBoids[i]))
            {
                cohesiveBoids.Add(closeBoids[i].position);
            }
        }
        cohesionVector = Vector3.ClampMagnitude(GetMeanVector(cohesiveBoids), 1);
        Debug.DrawRay(transform.position, cohesionVector*cohesionFactor, Color.green);

        targetVector += cohesionVector*cohesionFactor;
    }

    void ObstacleAvoidance()
    {
        RaycastHit[] rayHit = new RaycastHit[rayCount*2+1];
        Vector3[] rayAngle = new Vector3[rayCount*2+1];
        bool[] didRayHit = new bool[rayCount*2+1];
        bool seeObstacle = false;

        for (int i = 0; i < rayHit.Length; i++)
        {
            rayAngle[i] = Quaternion.Euler(0, Mathf.Pow(-1,i)*(rayFOV/(rayCount))*((i+1)/2), 0) * transform.forward;
            Debug.DrawRay(transform.position, rayAngle[i]*rayLength, Color.blue);
            
            didRayHit[i] = Physics.Raycast(transform.position, rayAngle[i], out rayHit[i], rayLength, Layers.Instance.obstacles);
            seeObstacle = seeObstacle || didRayHit[i];
        }

        if (seeObstacle)
        {
            for (int i = 0; i < rayAngle.Length; i++)
            {
                // To Do
            }
        }



        // if (didRayHit)
        // {
        //     targetVector = targetVector + rayAngle.normalized*avoidanceFactor*10;
        // }

        // if (!didRayHit)
        //     {
        //         Debug.DrawRay(transform.position, rayAngle*rayLength, Color.green);
        //         targetVector = targetVector + rayAngle.normalized*avoidanceFactor*10;
        //         return;
        //     }
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

    bool IsInFOV(Transform other)
    {
        float targetAngle = Vector3.Angle(other.position-transform.position, transform.forward);
        return (targetAngle <= fieldOfViewAngle);
    }

    Vector3 Clamp(Vector3 v, float max, float min)
    {
        double sm = v.sqrMagnitude;
        if(sm > (double)max * (double)max) return v.normalized * max;
        else if(sm < (double)min * (double)min) return v.normalized * min;
        return v;
    }

    private Vector3 GetMeanVector(List<Vector3> positions)
    {
        if(positions.Count == 0)
        {
            return Vector3.zero;
        }
    
        Vector3 meanVector = Vector3.zero;
    
        foreach(Vector3 pos in positions)
        {
            meanVector += pos;
        }
    
        return (meanVector / positions.Count);
    }
}
