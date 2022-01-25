using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidSpawner : MonoBehaviour
{
    public BoidMover boidPrefab;

    [SerializeField]
    private float spawnX = 10;
    [SerializeField]
    private float spawnZ = 10;
    [SerializeField]
    private int boidCount = 5;
    private int boidCounter;

    [SerializeField]
    private float spawnTimer = 2;
    private float spawnCooldown;
    void Start()
    {
        spawnCooldown = spawnTimer;
        boidCounter = 0;
    }

    void Update()
    {
        if (boidCounter < boidCount && spawnCooldown <= 0)
        {
            BoidMover boidInst = Instantiate(boidPrefab);
            boidInst.transform.parent = transform;
            boidInst.transform.position = new Vector3(Random.Range(spawnX/2,-spawnX/2),0,Random.Range(spawnZ/2,-spawnZ/2));
            boidInst.transform.Rotate(new Vector3(0, Random.Range(0, 360), 0));

            spawnCooldown = spawnTimer;
            boidCounter++;
        }
        else
        {
            spawnCooldown -= Time.deltaTime;
        }
    }
}
