using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Message 
{
    public Message(string name, float timestamp, GameObject sender, MessageType type) {
        this.name = name;
        this.timestamp = timestamp;
        this.sender = sender;
        this.type = type;
    }

    public enum MessageType
    {
        Interest,
        Data
    };
    
    public string name;
    public float timestamp;
    public GameObject sender;
    public MessageType type;
}
