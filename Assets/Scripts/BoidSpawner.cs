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
    void Start()
    {
        for (int i = 0; i < boidCount; i++)
        {
            BoidMover boidInst = Instantiate(boidPrefab);
            boidInst.transform.position = new Vector3(Random.Range(spawnX/2,-spawnX/2),0,Random.Range(spawnZ/2,-spawnZ/2));
            boidInst.transform.Rotate(new Vector3(0, Random.Range(0, 360), 0));
        }
    }
}
