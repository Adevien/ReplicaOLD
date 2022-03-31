using LiteNetLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class Player {
    public NetPeer Peer;
    public GameObject SimulatedPlayer;
    public Dictionary<int, InputCmd> InputCommands = new Dictionary<int, InputCmd>();
    public bool FirstPacket = true;
    public float LastTime;
}

