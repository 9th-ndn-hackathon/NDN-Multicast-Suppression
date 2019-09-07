using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NFDConsumer : MonoBehaviour
{

    GameObject broadcastRoot;
    public const float listenTime = 0.1F;
    public float m_supress = 0.02F;
    public string name;
    [SerializeField]
    float suppressionTime;
    Queue<Packet> incMulticastInterests;
    [SerializeField]
    int queueCount;

    //Current Interest wanting to be expressed.
    Packet currentInterest = null;
    int duplicateCount;

    void Awake()
    {
        //Set name of node
        if (name == "")
        {
            //Set to name of game object.
            name = gameObject.name;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        incMulticastInterests = new Queue<Packet>();
        broadcastRoot = gameObject.transform.parent.gameObject;
        float startDelay = Random.Range(0, 3f);
        StartCoroutine(DelayedStart(startDelay));
    }

    IEnumerator DelayedStart(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        StartCoroutine(GenerationRoutine());
    }

    IEnumerator GenerationRoutine()
    {
        float generationTime = MulticastManager.getInstanceOf().interestGenerationRate;
        int interestMax = MulticastManager.getInstanceOf().interestGenerationCount;
        int count = 0;
        while (count < interestMax)
        {
            Packet message = new Packet("/test/interest/" + count, Time.time, this.gameObject, Packet.PacketType.Interest);
            currentInterest = message;
            duplicateCount = 0;

            // Listen for the same interest and set supression time
            StartCoroutine(ListenRoutine());

            // broadcastRoot.BroadcastMessage("OnMulticastInterest", message, SendMessageOptions.DontRequireReceiver);
            count += 1;
            yield return new WaitForSeconds(generationTime);
        }
    }

    IEnumerator ProcessInterestDelay(float delay, Packet interest)
    {
        yield return new WaitForSeconds(delay);
        logMessage(Time.time+":Interest from " + interest.sender.name + " with name " + interest.name);

        // Check if interest exists in queue and add if it does
        enqueue(interest);
    }

    IEnumerator ProcessDataDelay(float delay, Packet data)
    {
        yield return new WaitForSeconds(delay);
        logMessage(Time.time + ":Data from " + data.sender.name + " with name " + data.name);
    }

    IEnumerator ListenRoutine()
  {
    yield return new WaitForSeconds(listenTime);
    if (duplicateCount > 1)
      m_supress = m_supress * 2;
    else if (duplicateCount == 1) {
      m_supress = 0;
    } else {
      yield break;
    }
  }

    void OnMulticastInterest(Packet interest)
    {
        if (interest.sender.name == gameObject.name)
        {
            return;
        }

        //Find the distance between sender and this node.  This is the propagation delay.
        float distance = Mathf.Abs(Vector3.Distance(interest.sender.transform.position, gameObject.transform.position));
        StartCoroutine(ProcessInterestDelay(distance / 1000f,interest));
    }

    void OnMulticastData(Packet data)
    {
        if (data.sender.name == gameObject.name)
        {
            return;
        }

        //Find the distance between sender and this node.  This is the propagation delay.
        float distance = Mathf.Abs(Vector3.Distance(data.sender.transform.position, gameObject.transform.position));
        StartCoroutine(ProcessDataDelay(distance / 1000f, data));
    }

    void logMessage(string message)
    {
        Debug.Log(name + ": " + message);
    }


    void enqueue(Packet interest) {
      bool inQueue = checkQueue(interest);
      if (!inQueue)
        incMulticastInterests.Enqueue(interest);
    }

    bool checkQueue(Packet interest) {
      bool inQueue = false;
      if (incMulticastInterests.Count != 0) {
        foreach (Packet p in incMulticastInterests) {
          if ((p.name).Equals(interest.name)) {
            inQueue = true;
            break;
          }
        }
      }
      return inQueue;
    }

}
