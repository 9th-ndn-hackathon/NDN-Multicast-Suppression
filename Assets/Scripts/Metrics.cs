using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Metrics : MonoBehaviour
{
  
    public int noInterests = 0;
    public int interestsRound = 0;
    public int roundsWithAWinner = 0;

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
        int[] rounds = new int[MulticastManager.getInstanceOf().interestGenerationCount];
        int i = 0;
        while(round < MulticastManager.getInstanceOf().interestGenerationCount)
        {
            yield return new WaitForSeconds(MulticastManager.getInstanceOf().interestGenerationRate);
            interestsRound = currentCount;
            if(interestsRound == 1)
            {
                roundsWithAWinner += 1;
            }
            rounds[i++] = interestsRound;
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
