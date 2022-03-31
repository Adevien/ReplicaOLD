using System.Collections.Generic;

namespace Replica.Structures {
    public struct InputCmd {
        public float DeliveryTime;
        public int LastAckedTick;
        public List<NetworkInput> Inputs;
    }
}
