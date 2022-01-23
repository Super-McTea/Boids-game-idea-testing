using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Layers : MonoBehaviour
{
    static private Layers instance;
    static public Layers Instance
    {
        get 
        {
            if (instance == null)
            {
                Debug.LogError("There is no Layers instance in the scene.");
            }
            return instance;
        }
    }

    public LayerMask boids;
    public LayerMask obstacles;
    public LayerMask players;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        instance = this;

        DontDestroyOnLoad(gameObject);
    }
}
