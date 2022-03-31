using Replica;
using Replica.Structures;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Replica.Demo {
    public class DemoHost : SimulationBehaviour {

        int _serverTick;

        Queue<InputCmd> _receivedInputCmds;

        [SerializeField] List<GameObject> _players;

        /// <summary>
        /// Only to be used locally for testing without networking added
        /// </summary>
        
        public DemoPeer _client;


        public void AddInput(InputCmd inputCmd) => _receivedInputCmds.Enqueue(inputCmd);
        /// <summary>
        /// end of references used for testing locally networking added
        /// </summary>

        private void Awake() {
            _receivedInputCmds = new Queue<InputCmd>();
        }

        public override void PreUpdate() {
            print("PREUPDATE_HOST");
        }

        public override void OnFixedStep() {
            while (_receivedInputCmds.Count > 0 && Time.time >= _receivedInputCmds.Peek().DeliveryTime) {
                InputCmd inputCmd = _receivedInputCmds.Dequeue();

                if (inputCmd.LastAckedTick + inputCmd.Inputs.Count - 1 >= _serverTick) {
                    for (int i = _serverTick > inputCmd.LastAckedTick ? _serverTick - inputCmd.LastAckedTick : 0; i < inputCmd.Inputs.Count; ++i) {
                        // MoveLocalEntity(ServerRb, inputCmd.Inputs[i]);
                        Physics.Simulate(NetRunner.StepTime);

                        ++_serverTick;

                        if (NetRunner.Instance.LagSim.IsPacketLoss) {
                            Snapshot snapshot;

                            snapshot.DeliveryTime = NetRunner.Instance.LagSim.AddWithLag(Time.time);
                            snapshot.Time = Time.time;
                            snapshot.Tick = _serverTick;
                            snapshot.Data = new SnapshotData[] {};

                            // < summary >
                            // Only to be used locally for testing without networking added
                            // </ summary >
                            _client.AddSnapshot(snapshot);
                        }
                    }
                }
            }
        }

        private void OnGUI() {
            GUI.Box(new Rect(5f, 155f, 180f, 25f), $"SERVER TICK {_serverTick}");
        }
    }
}

