using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NFDConsumer : MonoBehaviour
{

    GameObject broadcastRoot;
    public float listenTime;
    public string name;
    [SerializeField]
    float suppressionTime;
    Queue<Message> incMulticastInterests;

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
        incMulticastInterests = new Queue<Message>();
        broadcastRoot = gameObject.transform.parent.gameObject;

        Message message = new Message("/test/interest", 0.0f, this.gameObject, Message.MessageType.Interest);

        if (name == "Con A") {
            broadcastRoot.BroadcastMessage("OnMulticastInterest", message, SendMessageOptions.DontRequireReceiver);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
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

    void logMessage(string message) 
    {
        Debug.Log(name + ": " + message);
    }

}
