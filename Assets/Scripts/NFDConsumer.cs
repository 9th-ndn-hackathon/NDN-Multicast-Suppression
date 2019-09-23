using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NFDConsumer : NFDNode
{

    public GameObject packetTransmissionVisualizer;

    GameObject broadcastRoot;
    public float listenTime;
    public float m_supress = 0F;
    public int interestsSuppressed = 0;
    public string name;
    Queue<Packet> incMulticastInterests;
    [SerializeField]
    int queueCount;
    [SerializeField]
    int MAX_SUPPRESS = 32;
    List<string> dataRecv;

    //Current Interest wanting to be expressed.
    Packet currentInterest = null;
    int duplicateCount;
    Dictionary<string, bool> suppressMap;

    private float propagationDelayConstant; 

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
        suppressMap = new Dictionary<string, bool>();
        dataRecv = new List<string>();
        propagationDelayConstant = 1000f * Time.timeScale;
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
            currentInterest = message;
            duplicateCount = 0;
            suppressMap.Add("/test/interest/" + count, false);
            if (checkQueue(message))
            {
               // Debug.Log("Found in queue of " + name);
                count += 1;

            }else if (dataRecv.Contains(message.name))
            {
                //do nothing
            }
            else
            {
                // Listen for the same interest and set supression time
                StartCoroutine(ListenRoutine(message));
                StartCoroutine(SuppressionRoutine(message));
            }
 
            count += 1;
            yield return new WaitForSeconds(generationTime * (1f / Time.timeScale));
        }
    }

    IEnumerator SuppressionRoutine(Packet message)
    {
        if(m_supress > 0)
        {
            //clamp m_suppress
            m_supress = Mathf.Min(m_supress, MAX_SUPPRESS);
            float randomDelay = Random.Range(m_supress / 2, m_supress * 2);
            yield return new WaitForSeconds(randomDelay);
            if (!suppressMap[message.name])
            {
                logMessage(Time.time + ":"+ message.sender.name + " expresses interest " + message.name);
                sendInterest(message);
                duplicateCount += 1;
            }
            else
            {
                interestsSuppressed += 1;
            }

            //clamp 
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
            suppressMap[interest.name] = true;
        }
    }

    IEnumerator ProcessDataDelay(float delay, Packet data)
    {
        yield return new WaitForSeconds(delay);
        if (!dataRecv.Contains(data.name))
        {
            dataRecv.Add(data.name);
        }
        logMessage(Time.time + ":Data from " + data.sender.name + " with name " + data.name);
    }

    IEnumerator ListenRoutine(Packet message)
    {
        yield return new WaitForSeconds(listenTime * (1f/Time.timeScale));
        if (duplicateCount > 1)
            if (m_supress == 0)
            {
                m_supress = 4f * 1/Time.timeScale;
            }
            else
            {
                m_supress = m_supress * 2;
            }
        else if (duplicateCount == 1 && !suppressMap[message.name])
        {
            m_supress = 0;
        }
    }

    override public void OnMulticastInterest(Packet interest)
    {
        if (interest.sender.name == gameObject.name)
        {
            return;
        }

        //Find the distance between sender and this node.  This is the propagation delay.
        float distance = Mathf.Abs(Vector3.Distance(interest.sender.transform.position, gameObject.transform.position));
        logMessage("Waiting " + calculatePropagationDelay(distance));
        StartCoroutine(ProcessInterestDelay(calculatePropagationDelay(distance), interest));
    }

    override public void OnMulticastData(Packet data)
    {
        if (data.sender.name == gameObject.name)
        {
            return;
        }

        //Find the distance between sender and this node.  This is the propagation delay.
        float distance = Mathf.Abs(Vector3.Distance(data.sender.transform.position, gameObject.transform.position));
        logMessage("Waiting " + calculatePropagationDelay(distance));
        StartCoroutine(ProcessDataDelay(calculatePropagationDelay(distance), data));
    }

    void logMessage(string message)
    {
        //Debug.Log(name + ": " + message);
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
        emitPacketTransmissionVisual(propagationDelayConstant, 3f);
    }

    private void emitPacketTransmissionVisual(float growthRate, float lifeTime) {
        GameObject newTransmissionVisualizer = Instantiate(packetTransmissionVisualizer);
        newTransmissionVisualizer.transform.SetParent(gameObject.transform);
        newTransmissionVisualizer.transform.localPosition = Vector3.zero;
        CircleGrowth growthScript = newTransmissionVisualizer.GetComponent<CircleGrowth>();
        growthScript.setParameters(growthRate, lifeTime);
        growthScript.startGrowth();
    }

    private float calculatePropagationDelay(float distance) {
        return distance / propagationDelayConstant;
    }

}
