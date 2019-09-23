using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MulticastManager : MonoBehaviour
{
    public float interestGenerationRate;
    public int interestGenerationCount;
    public float listenTime;
    private List<NFDConsumer> nfdNodes;
    private static MulticastManager instance;
    public Text roundsText;
    public int currentRound= 0;
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
        nfdNodes = new List<NFDConsumer>();
        NFDConsumer[] nodes = GameObject.FindObjectsOfType<NFDConsumer>();
        nfdNodes.AddRange(nodes);
        StartCoroutine(CountRounds());

        
    }

    IEnumerator CountRounds()
    {
        currentRound = 0;
        float generationTime = interestGenerationRate;
        int interestMax = interestGenerationCount;
        while (currentRound < interestMax)
        {
            currentRound += 1;
            roundsText.text = "Rounds: " + currentRound;
            yield return new WaitForSeconds(generationTime);// * (1f / Time.timeScale));
        }
       

        
        
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
