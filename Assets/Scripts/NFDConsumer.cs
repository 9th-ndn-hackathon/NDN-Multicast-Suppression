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

        Message message = new Message();
        message.type = Message.MessageType.Interest;
        message.name = "test";
        message.sender = this.gameObject;

        broadcastRoot.BroadcastMessage("OnMulticastInterest", message, SendMessageOptions.DontRequireReceiver);
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

        Debug.Log("Interest from " + message.sender.name + " with interest " + message.name);
        //Find the distance between sender and this node.  This is the propagaton delay.
        float distance = Mathf.Abs(Vector3.Distance(message.sender.transform.position, gameObject.transform.position));
        Debug.Log("Distance is " + distance);
    }

    void OnMulticastData(Message message)
    {

    }

}
