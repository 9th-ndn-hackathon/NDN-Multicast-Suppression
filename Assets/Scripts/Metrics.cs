using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Metrics : MonoBehaviour
{
  
    public int noInterests = 0;
    public int interestsRound = 0;

    int currentCount = 0;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(WaitForRound());
    }

    // Update is called once per frame
    void Update()
    {
        

    }

    IEnumerator WaitForRound()
    {

        int round = MulticastManager.getInstanceOf().currentRound;
        while(round < MulticastManager.getInstanceOf().interestGenerationCount)
        {
            yield return new WaitForSeconds(MulticastManager.getInstanceOf().interestGenerationRate * (1f / Time.timeScale));
            interestsRound = currentCount;
            currentCount = 0;
            round += 1;
        }

    }
    void OnMulticastInterest(Packet interest)
    {
      noInterests++;
      currentCount++;
      //Debug.Log("Interest from " + interest.sender.name + " with name " + interest.name + "(Interest no: " + noInterests + ")");
    }
}
