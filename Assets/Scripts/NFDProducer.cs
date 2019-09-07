using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NFDProducer : MonoBehaviour
{

    GameObject broadcastRoot;
    public string name;

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
        broadcastRoot.BroadcastMessage("OnMulticastData", data, SendMessageOptions.DontRequireReceiver);
    }

    void OnMulticastInterest(Packet interest)
    {
        if(interest.sender.name == gameObject.name)
        {
            return;
        }
        
        //Find the distance between sender and this node.  This is the propagation delay.
        float distance = Mathf.Abs(Vector3.Distance(interest.sender.transform.position, gameObject.transform.position));
        StartCoroutine(ProcessInterestDelay(distance / 1000f * (1 / Time.timeScale), interest));

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void logMessage(string message) 
    {
        Debug.Log(name + ": " + message);
    }
}
