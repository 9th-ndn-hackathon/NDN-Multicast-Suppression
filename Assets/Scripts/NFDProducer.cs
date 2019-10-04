using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NFDProducer : NFDNode
{

    public GameObject packetTransmissionVisualizer;

    GameObject broadcastRoot;
    public string name;
    public float listenTime;
    public float m_supress = 0F;
    [SerializeField]
    int MAX_SUPPRESS;
    int dupDataCount;   //NOTE: This needs to be on a per name basis. This is just simplified for simulation.
    Dictionary<string, bool> supressDataMap;

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
        broadcastRoot = gameObject.transform.parent.gameObject;
    }

    IEnumerator ProcessInterestDelay(float delay, Packet interest)
    {
        yield return new WaitForSeconds(delay);
        logMessage(Time.time + ":Interest from " + interest.sender.name + " with name " + interest.name);
        Packet data = new Packet(interest.name, Time.time, this.gameObject, Packet.PacketType.Data);
        sendData(data);
        StartCoroutine(ListenRoutine(data));
    }

    override public void OnMulticastInterest(Packet interest)
    {
        if(interest.sender.name == gameObject.name)
        {
            return;
        }

        float delay = Random.Range(minPropDelay, maxPropDelay);
        StartCoroutine(ProcessInterestDelay(delay, interest));

    }

    IEnumerator ListenRoutine(Packet message)
    {

        //Listen for duplicates for this amount of time.
        yield return new WaitForSeconds(listenTime);

        //Start with 500ms suppression,  then double it up to MAX_SUPPRESS
        if (dupDataCount > 1)
            if (m_supress == 0)
            {
                m_supress = .5f;
            }
            else
            {
                m_supress = Mathf.Clamp(m_supress * 2, 0, MAX_SUPPRESS);
            }
        else if (dupDataCount == 1 && !supressDataMap[message.name])
        {
            //We heard our own interest only.  Declare ourselves to be the winner.
            m_supress = 0;
        }
    }

    public override void OnMulticastData(Packet data)
    {
        //Do nothing
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void logMessage(string message) 
    {
        //Debug.Log(name + ": " + message);
    }

    private void sendData(Packet data) {
        broadcastRoot.BroadcastMessage("OnMulticastData", data, SendMessageOptions.DontRequireReceiver);
        emitPacketTransmissionVisual(1000, 3.0f);
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
