using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NFDConsumer : NFDNode
{
    /** These fields are specific to this simulation and not transfered over to a real implementation **/
    //Prefab for circle effect
    public GameObject packetTransmissionVisualizer;
    //Number of suppressed interests from this node.
    public int interestsSuppressed = 0;
    //BroadcastMessage requires a root object
    GameObject broadcastRoot;
    //Used to simulate cached packets
    List<string> dataRecv;
    //Used to simulate a congestion window of interests
    [SerializeField]
    int interest_window;
    [SerializeField]
    bool showLogs = false;

    /**The below fields are specific to the algorithm and would be transfered over to a real implementation. **/

    //Time before the decision interval checks an interest for duplicates
    public float listenTime;
    //Time between when the decision interval decides the suppression level
    public float decisionInterval;
    //Size of the suppression window
    public float m_supress = 0F;
    //Incoming interest queue.  These interests should probably be stored in the PIT.
    Queue<Packet> incMulticastInterests;
    //Max suppression window
    [SerializeField]
    int MAX_SUPPRESS;
    //The current random delay being used.
    [SerializeField]
    float randomDelay = 0f;
    //The strategy info structure
    Dictionary<string, StrategyInfoEntry> StrategyInfo;

    private class StrategyInfoEntry
    {
        public int duplicates;  //count of duplicate interests
        public float sentTime;  //When this interest was sent
        public bool sentBeforeDuplicates;  //Winner nodes send their interests before others
        public float entryTime; //When this entry was created
        public bool isFinished; //Has this entry been around for at least listenTime
        public bool wasSuppressed;  //Was this interest suppressed.
        public bool processed; //Has this interest been processed by the decision interval
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
        StrategyInfo = new Dictionary<string, StrategyInfoEntry>();
        dataRecv = new List<string>();
        incMulticastInterests = new Queue<Packet>();
        broadcastRoot = gameObject.transform.parent.gameObject;
        StartCoroutine(GenerationRoutine());
    }

    /**
     * This routine kick starts our node to start sending blocks of interests.
     * 
     */
    IEnumerator GenerationRoutine()
    {
        //Wait for a random amount of time so nodes don't all start at the same time.
        float randomStartDelay = Random.Range(0f, .25f);
        yield return new WaitForSeconds(randomStartDelay);

        float generationTime = MulticastManager.getInstanceOf().interestGenerationRate;
        int interestMax = MulticastManager.getInstanceOf().interestGenerationCount;
        int interestCount = 0;
        StartCoroutine(DecisionEvent());  //Schedule the Decision Event
        while (interestCount < interestMax)
        {
            for (int i = 0; i < interest_window; i++)
            {
                //Create an interest and create a strategy info object for it
                Packet message = new Packet("/test/interest/" + interestCount, Time.time, this.gameObject, Packet.PacketType.Interest);
                StrategyInfoEntry entry = new StrategyInfoEntry
                {
                    duplicates = 0,
                    sentTime = -1f,
                    sentBeforeDuplicates = false,
                    entryTime = Time.time
                };
                StrategyInfo.Add(message.name, entry);

                //Look ahead case.  Did we see this interest in the past?
                if (checkQueue(message)) 
                {
                    logMessage("Found in queue:" + message.name);
                    interestsSuppressed += 1;
                    StrategyInfo[message.name].sentTime = Time.time;
                    StrategyInfo[message.name].isFinished = true;
                    StrategyInfo[message.name].wasSuppressed = true;
                }
                //See if the interest reply is already cached
                else if (dataRecv.Contains(message.name))
                {
                    logMessage("Data for " + name + " already recv");
                    interestsSuppressed += 1;
                    StrategyInfo[message.name].sentTime = Time.time;
                    StrategyInfo[message.name].isFinished = true;
                    StrategyInfo[message.name].wasSuppressed = true;
                }
                else
                {
                    if (m_supress > 0)
                    {
                        //Schedule the suppression event
                        StartCoroutine(SuppressionEvent(message));
                    }
                    else
                    {
                        //Send this interest immediately.
                        logMessage(Time.time + ":" + message.sender.name + " expresses interest " + message.name);
                        sendInterest(message);
                        StrategyInfo[message.name].duplicates += 1;
                        StrategyInfo[message.name].sentTime = Time.time;
                        StrategyInfo[message.name].sentBeforeDuplicates = true;
                        StrategyInfo[message.name].isFinished = true;
                    }
                }

                interestCount += 1;
            }

            //Wait some amount of time before sending the next block of interests.
            yield return new WaitForSeconds(generationTime);
        }

        //Print delay statistics now that this node is finished
        List<float> delays = new List<float>();
        foreach (string key in StrategyInfo.Keys)
        {
            float interestDelay = StrategyInfo[key].sentTime - StrategyInfo[key].entryTime;
            if(StrategyInfo[key].sentTime == -1f)
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

    /**
     * This event is fired after randomDelay and checks if a duplicate
     * interest was overheard or if the interest still needs to be sent.
     */
    IEnumerator SuppressionEvent(Packet message)
    {
        
        yield return new WaitForSeconds(randomDelay);  //Normally this would be done by NFD's scheduler.
        if (!StrategyInfo[message.name].wasSuppressed)
        {
            //No duplicates were heard so this interest is not suppressed. Send it now.
            logMessage(Time.time + ":"+ message.sender.name + " expresses interest " + message.name);
            sendInterest(message);
            StrategyInfo[message.name].duplicates += 1;  //This should be exactly 1 since no other duplicates were heard.
            StrategyInfo[message.name].sentTime = Time.time;
            
            if (StrategyInfo[message.name].duplicates == 1)
            {
                StrategyInfo[message.name].sentBeforeDuplicates = true;
            }
        }
        else
        {
            //This interest was suppressed.
            interestsSuppressed += 1;
        }
        
        //This interest is finished (either suppressed or sent)
        StrategyInfo[message.name].isFinished = true;
    }

    /**
     * This is used to simulate interest delay between nodes.  Analgous to an interest recieved event.
     */
    IEnumerator ProcessInterestDelay(float delay, Packet interest)
    {
        yield return new WaitForSeconds(delay);
        logMessage(Time.time + ":Interest from " + interest.sender.name + " with name " + interest.name);

        // Check if interest exists in queue and add if it does
        enqueue(interest);

        //Check if it is a duplicate of the interest we are currently interested in.
        if(StrategyInfo.ContainsKey(interest.name))
        {
            StrategyInfo[interest.name].duplicates += 1;
            if (!StrategyInfo[interest.name].wasSuppressed && !StrategyInfo[interest.name].isFinished)
            {
                StrategyInfo[interest.name].wasSuppressed = true;
            }
            
        }
    }

    /**
     * This is used to simulate data reply delay between nodes. Analgous to data recieved event.
     */
    IEnumerator ProcessDataDelay(float delay, Packet data)
    {
        yield return new WaitForSeconds(delay);
        if (!dataRecv.Contains(data.name))
        {
            dataRecv.Add(data.name);
        }
        //logMessage(Time.time + ":Data from " + data.sender.name + " with name " + data.name);
    }

    /**
     * This self scheduling event looks at a block of interests and decides if the node should suppress
     * multicast interests or set it to 0 (winner case).  It also determines the random delay for
     * the next block of interests.
     */
    IEnumerator DecisionEvent()
    {
        while (true)
        {

            //Gather list of interests to process
            List<string> processList = new List<string>();
            int total = 0;
            float totalInterestCount = 0;
            foreach (string key in StrategyInfo.Keys)
            {
                if (StrategyInfo[key].isFinished && !StrategyInfo[key].processed && StrategyInfo[key].entryTime + listenTime < Time.time)
                {
                    total += 1;
                    totalInterestCount += StrategyInfo[key].duplicates;
                    StrategyInfo[key].processed = true;
                    processList.Add(key);
                }
            }

            //Count the number of interests that were sent BEFORE duplicates were overheard.
            int countWins = 0;
            foreach (string name in processList)
            {
                if(StrategyInfo[name].sentTime != -1f && StrategyInfo[name].sentBeforeDuplicates)
                {
                    countWins += 1;
                }
            }

            //Determine win percentage.
            float percentage = 0f;
            if(countWins > 0 && total > 0)
            {
                percentage = (float)countWins / total;
            }

            if (percentage > .85f)
            {
                //This node is a winner for this block.
                m_supress = 0;
                //If duplicates were still overheard then multiple winners were found.  Add a bit of jitter to reconcile this.
                float avgDuplicates = totalInterestCount / total;
                if(avgDuplicates > 1.5f)
                {
                    m_supress = .25f;
                }
            }
            else if(processList.Count == 0)
            {
                //do nothing
            }
            else
            {
                //Start with an initial suppression,  then double it up to MAX_SUPPRESS
                if (m_supress == 0)
                {
                    m_supress = .25f;
                }
                else
                {
                    m_supress = Mathf.Clamp(m_supress * 2, 0, MAX_SUPPRESS);
                }
            }

            //Choose new randomDelay
            m_supress = Mathf.Min(m_supress, MAX_SUPPRESS);
            randomDelay = Random.Range(0, m_supress);

            //Have this event re-schedule itself.
            yield return new WaitForSeconds(decisionInterval);

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
