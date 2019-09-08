using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NFDConsumer : MonoBehaviour
{

    public GameObject packetTransmissionVisualizer;

    GameObject broadcastRoot;
    public const float listenTime = 0.1F;
    [SerializeField]
    float m_supress = 0F;
    public string name;
    Queue<Packet> incMulticastInterests;
    [SerializeField]
    int queueCount;

    //Current Interest wanting to be expressed.
    Packet currentInterest = null;
    int duplicateCount;
    bool suppressCurrentInterest = false;

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
        float startDelay = Random.Range(0, .1f * (1f / Time.timeScale));
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
            if (checkQueue(message))
            {
                Debug.Log("Found in queue of " + name);
                count += 1;
                yield return new WaitForSeconds(generationTime * (1f / Time.timeScale));
                continue;
            }
            currentInterest = message;
            duplicateCount = 0;
            suppressCurrentInterest = false;

            // Listen for the same interest and set supression time
            StartCoroutine(ListenRoutine());
            StartCoroutine(SuppressionRoutine(message));

            count += 1;
            yield return new WaitForSeconds(generationTime * (1f / Time.timeScale));
        }
    }

    IEnumerator SuppressionRoutine(Packet message)
    {
        if(m_supress > 0)
        {
            float randomDelay = Random.Range(0, m_supress);
            yield return new WaitForSeconds(randomDelay);
            if (!suppressCurrentInterest)
            {
                logMessage(Time.time + ":"+ message.sender.name + " expresses interest " + message.name);
                sendInterest(message);
                duplicateCount += 1;
            }

        }
        else
        {
            logMessage(Time.time + ":" + message.sender.name + " expresses interest " + message.name);
            sendInterest(message);
            duplicateCount += 1;
        }
    }
    IEnumerator ProcessInterestDelay(float delay, Packet interest)
    {
        yield return new WaitForSeconds(delay);
        logMessage(Time.time + ":Interest from " + interest.sender.name + " with name " + interest.name);

        // Check if interest exists in queue and add if it does
        enqueue(interest);

        //Check if it is a duplicate of the interest we are currently interested in.
        if(currentInterest != null && interest.name == currentInterest.name)
        {
            duplicateCount += 1;
            suppressCurrentInterest = true;
        }
    }

    IEnumerator ProcessDataDelay(float delay, Packet data)
    {
        yield return new WaitForSeconds(delay);
        logMessage(Time.time + ":Data from " + data.sender.name + " with name " + data.name);
    }

    IEnumerator ListenRoutine()
    {
        yield return new WaitForSeconds(listenTime * (1f/Time.timeScale));
        if (duplicateCount > 1)
            if (m_supress == 0)
            {
                m_supress = .05f * 1/Time.timeScale;
            }
            else
            {
                m_supress = m_supress * 2;
            }
        else if (duplicateCount == 1 && !suppressCurrentInterest)
        {
            m_supress = 0;
        }
        else
        {
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
        logMessage("Waiting " + (distance / 1000f * (1 / Time.timeScale)));
        StartCoroutine(ProcessInterestDelay(distance / 1000f * (1 / Time.timeScale), interest));
    }

    void OnMulticastData(Packet data)
    {
        if (data.sender.name == gameObject.name)
        {
            return;
        }

        //Find the distance between sender and this node.  This is the propagation delay.
        float distance = Mathf.Abs(Vector3.Distance(data.sender.transform.position, gameObject.transform.position));
        logMessage("Waiting " + (distance / 1000f * (1 / Time.timeScale)));
        StartCoroutine(ProcessDataDelay(distance / 1000f * (1 / Time.timeScale), data));
    }

    void logMessage(string message)
    {
        Debug.Log(name + ": " + message);
    }


    void enqueue(Packet interest)
    {
        bool inQueue = checkQueue(interest);
        if (!inQueue)
        {
            incMulticastInterests.Enqueue(interest);
            queueCount += 1;
        }
            
    }

    bool checkQueue(Packet interest)
    {
        bool inQueue = false;
        if (incMulticastInterests.Count != 0)
        {
            foreach (Packet p in incMulticastInterests)
            {
                if ((p.name).Equals(interest.name))
                {
                    inQueue = true;
                    break;
                }
            }
        }
        return inQueue;
    }

    private void sendInterest(Packet interest) {
        broadcastRoot.BroadcastMessage("OnMulticastInterest", interest, SendMessageOptions.DontRequireReceiver);
        emitPacketTransmissionVisual(100.0f, 100.0f);
    }

    private void emitPacketTransmissionVisual(float growthRate, float lifeTime) {
        GameObject newTransmissionVisualizer = Instantiate(packetTransmissionVisualizer);
        CircleGrowth growthScript = newTransmissionVisualizer.GetComponent<CircleGrowth>();
        growthScript.setParameters(this.gameObject.transform, growthRate, lifeTime);
        growthScript.startGrowth();
    }

}
