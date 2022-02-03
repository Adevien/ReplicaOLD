using Replica.Runtime;
using UnityEngine;

namespace Replica.Demo {
    public class Demo : MonoBehaviour {
        public NetworkEntityDemo LocalObject;
        public NetworkEntityDemo RemoteObject;

        public bool Initial = true;

        private void Awake() {
            LocalObject.IsLocal = false;
            RemoteObject.IsLocal = true;
        }

        Byter writer = new Byter();
        Byter reader = new Byter();

        private void Update() {
            if (Input.GetKeyDown(KeyCode.Space)) {

                LocalObject.Singles += 12.5f;

                LocalObject.WriteNetVars(writer, Initial);

                unsafe {
                    NativeUtils.MemCpyFast(reader.Data, writer.Data, writer.Position);
                }

                RemoteObject.ReadNetVars(reader, Initial);

                Debug.Log($"CLIENT_Ax FLAG COUNT {LocalObject.Flags} initial:({Initial})");
                Debug.Log($"CLIENT_Bx FLAG COUNT {RemoteObject.Flags} initial:({Initial})");

                Initial = false;

                writer.Reset();
                reader.Reset();
            }
        }
    }
}
