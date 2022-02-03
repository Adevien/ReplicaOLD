using Replica.Runtime;
using UnityEngine;

namespace Replica.Demo {
    public class NetworkEntityDemo : NetworkBehaviour {
        [NetVar(nameof(OnChanged))] public bool Booleanss { get; set; }

        [NetVar] public byte Bytes { get; set; }

        [NetVar] public sbyte SBytes { get; set; } = 5;

        [NetVar] public double Doubles { get; set; }

        [NetVar] public float Singles { get; set; }

        [NetVar] public int Integers { get; set; }

        [NetVar] public uint UnsignedIntegers { get; set; }

        [NetVar] public long Longs { get; set; }
        
        [NetVar] public ulong UnsignedLongs { get; set; }

        [NetVar] public short Shorts { get; set; }

        [NetVar] public ushort UnsignedShorts { get; set; }

        [NetVar] public string Strings { get; set; }

        public void OnChanged(bool value) {
            Debug.Log($"[NAME {Strings}] Property changed to value {value}");
        }
    }
}