using Replica.Runtime;
using UnityEngine;

namespace Replica.Demo
{
    public class Demo : MonoBehaviour
    {
        public CD LocalObject;
        public CD RemoteObject;

        public bool Initial = true;

        private void Awake()
        {
            LocalObject.IsLocal = false;
            RemoteObject.IsLocal = true;
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.Z))
            {
                NetBuffer buffer = new NetBuffer();

                buffer.WriteBool(true);

                bool initial = buffer.ReadBool();

                LocalObject.WriteNetVars(buffer, initial);
                RemoteObject.ReadNetVars(buffer, initial);
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (Initial)
                {
                    NetBuffer buffer = new NetBuffer();

                    buffer.WriteBool(true);

                    LocalObject.Health = 12.5f;

                    bool initial = buffer.ReadBool();

                    LocalObject.WriteNetVars(buffer, initial);
                    RemoteObject.ReadNetVars(buffer, initial);

                    Debug.Log($"CLIENT_A FLAG COUNT {LocalObject.Flags} initial:({initial})");
                    Debug.Log($"CLIENT_B FLAG COUNT {RemoteObject.Flags} initial:({initial})");

                    Initial = false;

                }
                else
                {
                    NetBuffer buffer = new NetBuffer();

                    buffer.WriteBool(false);

                    LocalObject.Health++;

                    bool initial = buffer.ReadBool();

                    LocalObject.WriteNetVars(buffer, initial);
                    RemoteObject.ReadNetVars(buffer, initial);

                    Debug.Log($"CLIENT_A FLAG COUNT {LocalObject.Flags} initial:({initial})");
                    Debug.Log($"CLIENT_B FLAG COUNT {RemoteObject.Flags} initial:({initial})");
                }
            }
        }
    }
}
