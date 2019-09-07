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

    void OnMulticastInterest(Message message)
    {
        if(message.sender.name == gameObject.name)
        {
            return;
        }
        
        //Find the distance between sender and this node.  This is the propagation delay.
        float distance = Mathf.Abs(Vector3.Distance(message.sender.transform.position, gameObject.transform.position));

        logMessage("Interest from " + message.sender.name + " with name " + message.name + " (distance " + distance + ")");

        Message data = new Message("/test/interest/data_response", 0.0f, this.gameObject, Message.MessageType.Data);

        broadcastRoot.BroadcastMessage("OnMulticastData", data, SendMessageOptions.DontRequireReceiver);
    }

    void OnMulticastData(Message message)
    {
        if(message.sender.name == gameObject.name)
        {
            return;
        }

        //Find the distance between sender and this node.  This is the propagation delay.
        float distance = Mathf.Abs(Vector3.Distance(message.sender.transform.position, gameObject.transform.position));

        logMessage("Data from " + message.sender.name + " with name " + message.name + " (distance " + distance + ")");
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
