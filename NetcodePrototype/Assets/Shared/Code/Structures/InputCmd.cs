using LiteNetLib.Utils;

[System.Serializable]
public struct InputCmd : INetSerializable {
    public int Tick;
    public float ReceivedTime;
    public float ReceivedDelta;

    public void Serialize(NetDataWriter writer) {
        writer.Put(Tick);
        writer.Put(ReceivedTime);
        writer.Put(ReceivedDelta);
    }

    public void Deserialize(NetDataReader reader) {
        Tick = reader.GetInt();
        ReceivedTime = reader.GetFloat();
        ReceivedDelta = reader.GetFloat();
    }
}
