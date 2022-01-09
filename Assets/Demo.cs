using Replica.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets
{
    public class Demo : MonoBehaviour
    {
        public CD clientA;
        public CD clientB;

        public bool Initial = true;

        private void Awake()
        {
            clientA.IsLocal = false;
            clientB.IsLocal = true;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (Initial)
                {
                    NetBuffer buffer = new NetBuffer();

                    buffer.WriteBool(true);

                    clientA.Health = 12.5f;

                    bool initial = buffer.ReadBool();

                    clientA.WriteNetVars(buffer, initial);
                    clientB.ReadNetVars(buffer, initial);

                    Debug.Log($"CLIENT_A FLAG COUNT {clientA.Flags} initial:({initial})");
                    Debug.Log($"CLIENT_B FLAG COUNT {clientB.Flags} initial:({initial})");

                    Initial = false;

                }
                else
                {
                    NetBuffer buffer = new NetBuffer();

                    buffer.WriteBool(false);

                    clientA.Health++;

                    bool initial = buffer.ReadBool();

                    clientA.WriteNetVars(buffer, initial);
                    clientB.ReadNetVars(buffer, initial);

                    Debug.Log($"CLIENT_A FLAG COUNT {clientA.Flags} initial:({initial})");
                    Debug.Log($"CLIENT_B FLAG COUNT {clientB.Flags} initial:({initial})");
                }
            }
        }
    }
}
