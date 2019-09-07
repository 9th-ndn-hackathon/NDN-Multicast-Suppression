using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Metrics : MonoBehaviour
{
   int noInterests = 0;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnMulticastInterest(Packet interest)
    {
      noInterests++;
      Debug.Log("Interest from " + interest.sender.name + " with name " + interest.name + "(Interest no: " + noInterests + ")");
    }
}
