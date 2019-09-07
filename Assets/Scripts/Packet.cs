using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Packet
{
    public Packet(string name, float timestamp, GameObject sender, PacketType type) {
        this.name = name;
        this.timestamp = timestamp;
        this.sender = sender;
        this.type = type;
    }

    public enum PacketType
    {
        Interest,
        Data
    };
    
    public string name;
    public float timestamp;
    public GameObject sender;
    public PacketType type;
}
