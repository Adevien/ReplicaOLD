using Replica;
using Replica.Structures;
using UnityEngine;

namespace Replica.Demo {
    public class SharedEntity : NetworkBehaviour {

        public Transform View;

        //NetworkRigidbodyController _cc;

        [NetVar] public float Yaw { get; set; }
        [NetVar] public float Pitch { get; set; }

        public override void OnFixedStep() {

            Vector3 direction = default;

            if (GetInput(out NetworkInput input)) {

                if (input.IsDown(Key.BTN_FORWARD)) direction += transform.forward;
                if (input.IsDown(Key.BTN_BACKWARD)) direction -= transform.forward;
                if (input.IsDown(Key.BTN_LEFTWARD)) direction -= transform.right;
                if (input.IsDown(Key.BTN_RIGHTWARD)) direction += transform.right;

                // if (input.IsDown(Key.BTN_UPWARD))
                // _cc.Jump();
            }

            // _cc.Move(direction.normalized * 3f);

            transform.rotation = Quaternion.Euler(0, Yaw, 0);
        }

        //TODO : Abstract render with custom view replication
        public override void Render() {
            //View.position = Vector3.Lerp(View.position, _cc.Position, Runner.Delta);
            //View.position = Interpolator.Lerp(View.position, _cc.Position);
        }
    }
}


public static class Interpolator {
    public static Vector3 Lerp(Vector3 start, Vector3 end) {
        return Vector3.zero;
    }
}

public class NetworkRigidbodyController {

    public Vector3 Position { get; set; }
    public void Move(Vector3 move) {

    }

    public void Jump() { }
}