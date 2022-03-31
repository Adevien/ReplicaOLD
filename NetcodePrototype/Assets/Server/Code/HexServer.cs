using LiteNetLib;
using LiteNetLib.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HexServer : MonoBehaviour {
    NetPacketProcessor _Processor = new NetPacketProcessor();

    EventBasedNetListener _Events = new EventBasedNetListener();

    [SerializeField] List<Player> _Players = new List<Player>();

    NetManager _Manager;

    private void Awake() {
        _Manager = new NetManager(_Events);

        _Events.PeerConnectedEvent += (peer) => {
            Debug.Log($"Peer {peer.EndPoint} connected");
            if (!_Players.Any(x=> x.Peer.EndPoint == peer.EndPoint)) {
                _Players.Add(new Player() { Peer = peer });
            }
        };

        _Events.PeerDisconnectedEvent += (peer, di) => {
            Debug.Log($"Peer {peer.EndPoint} disconnected. Reason {di.Reason}");
            _Players.Remove(_Players.First(x => x.Peer.EndPoint == peer.EndPoint));
        };

        _Events.ConnectionRequestEvent += (request) => request.AcceptIfKey("hex_arena");
        _Events.NetworkReceiveEvent += (peer, reader, channel, deliveryMethod) => _Processor.ReadAllPackets(reader, peer);

        RegisterMessages();

        Debug.Log($"HOST NetManager status: {(_Manager.Start(2500) ? "RUNNING" : "FAILED")}");
    }

    public int TickRate = 32;

    float BaseFixedDelta => 1.0f / TickRate;

    float _accTime;

    public int ServerTick;

    public float _currentLocalTime;

    public void Update() {
        _currentLocalTime = Time.time;
        _accTime += Time.deltaTime;

        if (_Manager.IsRunning) {
            _Manager.PollEvents();

            while (_accTime > BaseFixedDelta) {
                _accTime -= BaseFixedDelta;

                Snapshot snapshot;

                snapshot.Tick = ServerTick;

                foreach (var p in _Players) {
                    if (p.InputCommands.TryGetValue(ServerTick, out InputCmd input)) {
                        snapshot.InputRecvDelta = input.ReceivedDelta;
                        snapshot.InputBuffDelay = _currentLocalTime - input.ReceivedTime;
                        p.InputCommands.Remove(ServerTick);
                    } else {
                        Debug.LogWarning($"INPUT MISSED SERVER (TICK {ServerTick}) (TIME {_currentLocalTime})");
                        snapshot.InputRecvDelta = BaseFixedDelta * 2f;
                        snapshot.InputBuffDelay = -BaseFixedDelta;
                    }

                    _Processor.SendNetSerializable(p.Peer, ref snapshot, DeliveryMethod.Unreliable);
                }

                ServerTick++;
            }
        }
    }

    void RegisterMessages() {
        _Processor.SubscribeNetSerializable<InputCmd, NetPeer>((input, peer) => {

           if(_Players.Any(x => x.Peer.EndPoint == peer.EndPoint)) {
                var player = _Players.First(x => x.Peer.EndPoint == peer.EndPoint);

                var newInput = new InputCmd();
                newInput.Tick = input.Tick;
                newInput.ReceivedTime = _currentLocalTime;

                if (player.FirstPacket) 
                {
                    newInput.ReceivedDelta = BaseFixedDelta;

                    player.FirstPacket = false;
                } 
                else 
                {
                    player.LastTime = newInput.ReceivedTime;
                    newInput.ReceivedDelta = _currentLocalTime - player.LastTime;
                }

                player.InputCommands.Add(newInput.Tick, newInput);

            }
           
        });
    }

    private void OnDestroy() {
        _Manager.Stop();
        _Manager = null;
    }
}
