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
    Queue<Interest> incMulticastInterests;

    // Start is called before the first frame update
    void Start()
    {
        incMulticastInterests = new Queue<Interest>();
        broadcastRoot = gameObject.transform.parent.gameObject;

        Message message = new Message();
        Interest interest = new Interest(name);
        message.interest = interest;
        message.sender = this.gameObject;

        broadcastRoot.BroadcastMessage("OnMulticastInterest", message, SendMessageOptions.DontRequireReceiver);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnMulticastInterest(Message message)
    {
        if(message.sender.name != gameObject.name)
        {
            Debug.Log("Message from " + message.sender.name + " with interest " + message.interest.name);
        }
    }
}
