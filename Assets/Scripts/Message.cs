using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Message 
{
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
