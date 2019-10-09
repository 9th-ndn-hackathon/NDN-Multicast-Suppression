using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NFDConsumer : NFDNode
{

    public GameObject packetTransmissionVisualizer;

    GameObject broadcastRoot;
    public float listenTime;
    public float decisionInterval;
    public float m_supress = 0F;
    public int interestsSuppressed = 0;
    public string name;
    Queue<Packet> incMulticastInterests;
    [SerializeField]
    int queueCount;
    [SerializeField]
    int MAX_SUPPRESS;
    List<string> dataRecv;
    [SerializeField]
    int interest_window;
    [SerializeField]
    int latestSequence = 0;
    [SerializeField]
    float randomDelay = 0f;
    [SerializeField]
    bool showLogs = false;
    Dictionary<string, DuplicateMapEntry> duplicateMap;

    private class DuplicateMapEntry
    {
        public int count;
        public float sentTime;
        public bool sentBeforeDuplicates;
        public float entryTime;
        public bool isFinished;
        public bool wasSuppressed;
        public bool processed;
    }

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
        interest_window = 10;// Random.Range(2, 10);
        duplicateMap = new Dictionary<string, DuplicateMapEntry>();
        dataRecv = new List<string>();
        incMulticastInterests = new Queue<Packet>();
        broadcastRoot = gameObject.transform.parent.gameObject;
        StartCoroutine(GenerationRoutine());
    }

    IEnumerator GenerationRoutine()
    {
        float randomStartDelay = Random.Range(0f, .25f);
        yield return new WaitForSeconds(randomStartDelay);

        float generationTime = MulticastManager.getInstanceOf().interestGenerationRate;
        int interestMax = MulticastManager.getInstanceOf().interestGenerationCount;
        int count = 0;
        StartCoroutine(DecisionRoutine());
        while (count < interestMax)
        {
            for (int i = 0; i < interest_window; i++)
            {
                Packet message = new Packet("/test/interest/" + count, Time.time, this.gameObject, Packet.PacketType.Interest);
                DuplicateMapEntry entry = new DuplicateMapEntry
                {
                    count = 0,
                    sentTime = -1f,
                    sentBeforeDuplicates = false,
                    entryTime = Time.time
                };
                duplicateMap.Add(message.name, entry);
                if (checkQueue(message))
                {
                    logMessage("Found in queue:" + message.name);
                    interestsSuppressed += 1;
                    duplicateMap[message.name].sentTime = Time.time;
                    duplicateMap[message.name].isFinished = true;
                    duplicateMap[message.name].wasSuppressed = true;
                }
                else if (dataRecv.Contains(message.name))
                {
                    logMessage("Data for " + name + " already recv");
                    interestsSuppressed += 1;
                    duplicateMap[message.name].sentTime = Time.time;
                    duplicateMap[message.name].isFinished = true;
                    duplicateMap[message.name].wasSuppressed = true;
                }
                else
                {
                    if (m_supress > 0)
                    {

                        StartCoroutine(SuppressionRoutine(message));
                    }
                    else
                    {
                        logMessage(Time.time + ":" + message.sender.name + " expresses interest " + message.name);
                        sendInterest(message);
                        duplicateMap[message.name].count += 1;
                        duplicateMap[message.name].sentTime = Time.time;
                        duplicateMap[message.name].sentBeforeDuplicates = true;
                        duplicateMap[message.name].isFinished = true;
                    }
                }

                count += 1;
                latestSequence = count;
            }

            yield return new WaitForSeconds(generationTime);
        }

        //Print delay statistics
        List<float> delays = new List<float>();
        foreach (string key in duplicateMap.Keys)
        {
            float interestDelay = duplicateMap[key].sentTime - duplicateMap[key].entryTime;
            if(duplicateMap[key].sentTime == -1f)
            {
                interestDelay = 0f;  //interest was suppressed.  Treat as no delay in the interest.
            }
            delays.Add(interestDelay);
        }
        string delayString = "";
        foreach(float delay in delays)
        {
            delayString += ("," + delay);
        }
        print(gameObject.name + ":" + delayString);
    }

    IEnumerator SuppressionRoutine(Packet message)
    {
        
        yield return new WaitForSeconds(randomDelay);
        if (!duplicateMap[message.name].wasSuppressed)
        {
            logMessage(Time.time + ":"+ message.sender.name + " expresses interest " + message.name);
            sendInterest(message);
            duplicateMap[message.name].count += 1;
            duplicateMap[message.name].sentTime = Time.time;
            
            if (duplicateMap[message.name].count == 1)
            {
                duplicateMap[message.name].sentBeforeDuplicates = true;
            }
        }
        else
        {
            interestsSuppressed += 1;
        }
        duplicateMap[message.name].isFinished = true;
    }

    IEnumerator ProcessInterestDelay(float delay, Packet interest)
    {
        yield return new WaitForSeconds(delay);
        logMessage(Time.time + ":Interest from " + interest.sender.name + " with name " + interest.name);

        // Check if interest exists in queue and add if it does
        enqueue(interest);

        //Check if it is a duplicate of the interest we are currently interested in.
        if(duplicateMap.ContainsKey(interest.name))
        {
            duplicateMap[interest.name].count += 1;
            if (!duplicateMap[interest.name].wasSuppressed)
            {
                duplicateMap[interest.name].wasSuppressed = true;
            }
            
        }
    }

    IEnumerator ProcessDataDelay(float delay, Packet data)
    {
        yield return new WaitForSeconds(delay);
        if (!dataRecv.Contains(data.name))
        {
            dataRecv.Add(data.name);
        }
        //logMessage(Time.time + ":Data from " + data.sender.name + " with name " + data.name);
    }

    IEnumerator DecisionRoutine()
    {
        while (true)
        {
            //Listen for duplicates for this amount of time.
            yield return new WaitForSeconds(decisionInterval);

            //Remove old duplicate map entries
            List<string> removals = new List<string>();
            int total = 0;
            float totalInterestCount = 0;
            foreach (string key in duplicateMap.Keys)
            {
                if (duplicateMap[key].isFinished && !duplicateMap[key].processed && duplicateMap[key].entryTime + listenTime < Time.time)
                {
                    total += 1;
                    totalInterestCount += duplicateMap[key].count;
                    duplicateMap[key].processed = true;
                    removals.Add(key);
                }
            }
            int countWins = 0;
            foreach (string name in removals)
            {
                if(duplicateMap[name].sentTime != -1f && duplicateMap[name].sentBeforeDuplicates)
                {
                    countWins += 1;
                }
            }

            float percentage = 0f;
            if(countWins > 0 && total > 0)
            {
                percentage = (float)countWins / total;
            }

            if (percentage > .85f)
            {
                //We were the first interest to get sent.  Declare ourselves to be the winner.
                m_supress = 0;
                float avgDuplicates = totalInterestCount / total;
                print(gameObject.name + " Win percentage:" + percentage + " with avg duplicate count:"+avgDuplicates);
                if(avgDuplicates > 1.5f)
                {
                    //There are multiple winners.  Add some jitter.
                    m_supress = .25f;
                }
            }
            else if(removals.Count == 0)
            {
                //do nothing
            }
            else
            {
                //Start with 500ms suppression,  then double it up to MAX_SUPPRESS
                if (m_supress == 0)
                {
                    m_supress = .5f;
                }
                else
                {
                    m_supress = Mathf.Clamp(m_supress * 2, 0, MAX_SUPPRESS);
                }
            }

            //Choose new randomDelay
            m_supress = Mathf.Min(m_supress, MAX_SUPPRESS);
            //float randomDelay = Random.Range(m_supress / 2, m_supress * 2);
            randomDelay = Random.Range(0, m_supress);

        }
    }

    override public void OnMulticastInterest(Packet interest)
    {
        if (interest.sender.name == gameObject.name)
        {
            return;
        }

        //Abstracting away the AP and using typical propagation delays.
        float delay = Random.Range(minPropDelay, maxPropDelay);
        StartCoroutine(ProcessInterestDelay(delay, interest));
    }

    override public void OnMulticastData(Packet data)
    {
        if (data.sender.name == gameObject.name)
        {
            return;
        }

        //Abstracting away the AP and using typical propagation delays.
        float delay = Random.Range(minPropDelay, maxPropDelay);
        StartCoroutine(ProcessDataDelay(delay, data));
    }

    void logMessage(string message)
    {
        if(showLogs)
            print(name + ": " + message);
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
        //Check the incoming interest queue.  Currently there is no set limit
        //however in the proper implementation there would be a limit.
        //This queue should not be very large as we don't want unsatisfied interests in it.
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
        emitPacketTransmissionVisual(1000, 3f);
    }

    private void emitPacketTransmissionVisual(float growthRate, float lifeTime) {
        GameObject newTransmissionVisualizer = Instantiate(packetTransmissionVisualizer);
        newTransmissionVisualizer.transform.SetParent(gameObject.transform);
        newTransmissionVisualizer.transform.localPosition = Vector3.zero;
        CircleGrowth growthScript = newTransmissionVisualizer.GetComponent<CircleGrowth>();
        growthScript.setParameters(growthRate, lifeTime);
        growthScript.startGrowth();
    }



}
