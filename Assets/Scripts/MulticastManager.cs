using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MulticastManager : MonoBehaviour
{
    public float interestGenerationRate;
    public int interestGenerationCount;
    public float listenTime;
    private List<NFDNode> nfdNodes;
    private static MulticastManager instance;

    public static MulticastManager getInstanceOf()
    {
        return instance;
    }

    private void Awake()
    {
        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        nfdNodes = new List<NFDNode>();
        NFDNode[] nodes = GameObject.FindObjectsOfType<NFDNode>();
        nfdNodes.AddRange(nodes);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
