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

    void OnMulticastInterest(Message message)
    {
      Debug.Log("Message from " + message.sender.name + " with interest " + message.name);
      noInterests++;
      Debug.Log("Interest no: " + noInterests);

    }
}
