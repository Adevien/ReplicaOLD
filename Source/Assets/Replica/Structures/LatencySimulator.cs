using System;

namespace Replica.Structures {
    [Serializable]
    public struct LatencySimulator {
        [UnityEngine.Range(0.0f, 0.4f)] public float Latency;
        [UnityEngine.Range(0, 1)] public float PacketLoss;

        public bool IsPacketLoss => UnityEngine.Random.value > PacketLoss;

        public float AddWithLag(float value) => value + Latency;

        public float AddRandomLatency(float value) { 
            Latency = UnityEngine.Random.Range(0.05f, 0.4f); 
            return value + Latency;
        }
    }
}
