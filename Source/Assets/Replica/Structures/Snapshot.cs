namespace Replica.Structures {
    public struct Snapshot {
        public int Tick;
        public float Time;
        public float DeliveryTime;
        public SnapshotData[] Data;
    }
}
