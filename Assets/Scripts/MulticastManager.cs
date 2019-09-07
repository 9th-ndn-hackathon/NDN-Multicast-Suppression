using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MulticastManager : MonoBehaviour
{
    public float interestGenerationRate;
    public int interestGenerationCount;
    public float timeMultiplier;
    private static MulticastManager instance;

    public static MulticastManager getInstanceOf()
    {
        return instance;
    }
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
