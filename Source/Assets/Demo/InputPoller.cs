using Replica.Structures;
using Replica.Utils;
using UnityEngine;

namespace Replica.Demo {
    public class InputPoller : MonoBehaviour, IInputEvent {

        public void OnInput(ref NetworkInput input) {

            if (Input.GetKey(KeyCode.W))
                input.Buttons |= Key.BTN_FORWARD;

            if (Input.GetKey(KeyCode.S))
                input.Buttons |= Key.BTN_BACKWARD;

            if (Input.GetKey(KeyCode.A))
                input.Buttons |= Key.BTN_LEFTWARD;

            if (Input.GetKey(KeyCode.D))
                input.Buttons |= Key.BTN_RIGHTWARD;

            if (Input.GetKey(KeyCode.Space))
                input.Buttons |= Key.BTN_UPWARD;
        }
    }
}
