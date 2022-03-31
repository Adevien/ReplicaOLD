using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public struct Snapshot : INetSerializable {
    public float InputRecvDelta;
    public float InputBuffDelay;
    public int Tick;
    //public Vector3[] Position;
    //public Quaternion[] Rotation;

    public void Deserialize(NetDataReader reader) {
        Tick = reader.GetInt();
        InputRecvDelta = reader.GetFloat();
        InputBuffDelay = reader.GetFloat();

        //var playerCount = reader.GetInt();
        //Position = new Vector3[playerCount];
        //for (int i = 0; i < playerCount; i++) {
        //    Position[i] = new Vector3(reader.GetFloat(), reader.GetFloat(), reader.GetFloat());
        //}

        //Rotation = new Quaternion[playerCount];
        //for (int i = 0; i < playerCount; i++) {
        //    Rotation[i] = new Quaternion(reader.GetFloat(), reader.GetFloat(), reader.GetFloat(), reader.GetFloat());
        //}
    }

    public void Serialize(NetDataWriter writer) {
        writer.Put(Tick);
        writer.Put(InputRecvDelta);
        writer.Put(InputBuffDelay);

        //writer.Put(Position.Length);

        //foreach (var position in Position) {
        //    writer.Put(position.x);
        //    writer.Put(position.y);
        //    writer.Put(position.z);
        //}

        //foreach (var rotation in Rotation) {
        //    writer.Put(rotation.x);
        //    writer.Put(rotation.y);
        //    writer.Put(rotation.z);
        //    writer.Put(rotation.w);
        //}
    }
}