using UnityEngine;
using LiteNetLib;
using LiteNetLib.Utils;

public class HexConnection : MonoBehaviour {

    NetPacketProcessor _Processor = new NetPacketProcessor();

    EventBasedNetListener _Events = new EventBasedNetListener();

    NetPeer Peer;

    NetManager _Manager;

    private void Awake() {
        _Manager = new NetManager(_Events);

        _Events.PeerConnectedEvent += (peer) => Debug.Log($"We connected to server {peer.EndPoint}");
        _Events.PeerDisconnectedEvent += (peer, di) => Debug.Log($"We disconnected from server {peer.EndPoint}. Reason {di.Reason}");
        _Events.NetworkReceiveEvent += (peer, reader, channel, deliveryMethod) => _Processor.ReadAllPackets(reader);

        RegisterMessages();

        RecvDelta.Initialize(TickRate);
        BuffDelay.Initialize(TickRate);
    }

    public bool ConnectToServer;

    public bool Connect() {
        if (_Manager.Start()) {
            Peer = _Manager.Connect("127.0.0.1", 2500, "hex_arena");
            Debug.Log("CLIENT: Started on port (2500).");
            return true;
        } else {
            Debug.Log("CLIENT:: Failed to start manager.");
            return false;
        }
    }

    public int TickRate = 32;

    float BaseFixedDelta => 1.0f / TickRate;

    float _accTime;

    public float _currentLocalTime;

    public bool FirstState = true;

    public float TimeScale = 1;

    public int ClientLocalTick;

    public float TargetInputDelay;

    public float InputErrorDelay;

    StdDev RecvDelta;
    StdDev BuffDelay;

    public float PositiveError = 0.003125f;
    public float NegativeError = 0.003125f;

    public void Update() {
        _currentLocalTime = Time.time;
        _accTime += Time.deltaTime;

        if(ConnectToServer && !_Manager.IsRunning) {
            ConnectToServer = Connect();
        }

        if (_Manager.IsRunning) {
            _Manager.PollEvents();

            InputErrorDelay = TargetInputDelay - -BuffDelay.Mean;

            if (InputErrorDelay > PositiveError) {
                TimeScale = 0.95f;

            } else if (InputErrorDelay < -NegativeError) {
                TimeScale = 1.05f;

            } else {
                TimeScale = 1.00f;
            }

            while (_accTime > TimeScale * BaseFixedDelta) {
                _accTime -= TimeScale * BaseFixedDelta;

                if (!FirstState) {
                    InputCmd cmd;
                    cmd.Tick = ClientLocalTick;
                    cmd.ReceivedTime = 0;
                    cmd.ReceivedDelta = 0;

                    _Processor.SendNetSerializable(_Manager, ref cmd, DeliveryMethod.Unreliable);

                    ClientLocalTick++;
                }
            }
        }
    }

    void RegisterMessages() {
        _Processor.SubscribeNetSerializable<Snapshot>((snapshot) => {

            if (FirstState) {

                ClientLocalTick = snapshot.Tick;
                _accTime += 0.05f;
                FirstState = false;
            }

            RecvDelta.Integrate(snapshot.InputRecvDelta);
            BuffDelay.Integrate(snapshot.InputBuffDelay);

            var std_dev = Mathf.Sqrt(RecvDelta.Variance + BuffDelay.Variance);

            TargetInputDelay = (BaseFixedDelta * 3) * std_dev;

        });
    }

    private void OnDestroy() {
        _Manager.Stop();
        _Manager = null;
        Peer = null;
    }
}
