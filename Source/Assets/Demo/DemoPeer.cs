using Replica;
using Replica.Structures;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Replica.Demo {
    public class DemoPeer : SimulationBehaviour {

        Queue<Snapshot> _receivedSnapshots;

        PredictionStep[] _simulationSteps;

        int _clientTick;

        int _clientLastAckedTick;

        InputCmd inputCmd;

        [SerializeField] GameObject _localPlayer;

        /// <summary>
        /// Only to be used locally for testing without networking added
        /// </summary>

        public DemoHost _server;

        public void AddSnapshot(Snapshot snapshot) => _receivedSnapshots.Enqueue(snapshot);
        /// <summary>
        /// end of references used for testing locally networking added
        /// </summary>

        private void Awake() {
            _simulationScene = SceneManager.LoadScene("simulation_scene", new LoadSceneParameters(LoadSceneMode.Additive, LocalPhysicsMode.Physics3D)).GetPhysicsScene();
            SceneManager.MoveGameObjectToScene(_localPlayer, SceneManager.GetSceneByName("simulation_scene"));

            _simulationSteps = new PredictionStep[NetRunner.StepRate];
            _receivedSnapshots = new Queue<Snapshot>();
        }

        PhysicsScene _simulationScene;

        public override void PreUpdate() {
            print("PREUPDATE_PEER");
            //TODO ADD GETINPUTS FANCINESS
        }

        public override void OnFixedStep() {
            int simulationTick = _clientTick % NetRunner.StepRate;

            _simulationSteps[simulationTick].Input = NetRunner.Instance._storedInputs;

            SimulateStep(ref _simulationSteps[simulationTick]);

            if (NetRunner.Instance.LagSim.IsPacketLoss) {
                inputCmd.DeliveryTime = NetRunner.Instance.LagSim.AddWithLag(Time.time);
                inputCmd.LastAckedTick = _clientLastAckedTick;
                inputCmd.Inputs = new List<NetworkInput>();

                for (int tick = inputCmd.LastAckedTick; tick <= _clientTick; ++tick)
                    inputCmd.Inputs.Add(_simulationSteps[tick % NetRunner.StepRate].Input);

                _server.AddInput(inputCmd);
            }

            ++_clientTick;

            if (_receivedSnapshots.Count > 0 && Time.time >= _receivedSnapshots.Peek().DeliveryTime) {
                Snapshot snapshot = _receivedSnapshots.Dequeue();

                while (_receivedSnapshots.Count > 0 && Time.time >= _receivedSnapshots.Peek().DeliveryTime)
                    snapshot = _receivedSnapshots.Dequeue();

                _clientLastAckedTick = snapshot.Tick;

                //ClientRb.position = snapshot.Position;
                //ClientRb.rotation = snapshot.Rotation;
                //ClientRb.velocity = snapshot.Velocity;
                //ClientRb.angularVelocity = snapshot.AngularVelocity;

                //Debug.Log("REWIND " + snapshot.Tick + " (rewinding " + (_clientTick - snapshot.Tick) + " ticks)");

                int TicksToRewind = snapshot.Tick;

                // if(TicksToRewind > 16) //snap; 
                //TODO maybe add snapping

                while (TicksToRewind < _clientTick) {
                    int rewindTick = TicksToRewind % NetRunner.StepRate;
                    SimulateStep(ref _simulationSteps[rewindTick]/*, ClientRb*/);
                    ++TicksToRewind;
                }
            }
        }

        void SimulateStep(ref PredictionStep state) {
            state.data.Position = _localPlayer.GetComponent<Rigidbody>().position;
            state.data.Rotation = _localPlayer.GetComponent<Rigidbody>().rotation;

            //_localPlayer.GetComponent<SharedEntity>().Move(state.Input);
           
            _simulationScene.Simulate(NetRunner.StepTime);
        }

        public override void Render() {
            print("RENDER_");
        }

        private void OnGUI() {
            GUI.Box(new Rect(5f, 65f, 180f, 25f), $"STORED COMMANDS {inputCmd.Inputs?.Count}");
            GUI.Box(new Rect(5f, 95f, 180f, 25f), $"LAST TICK {_clientLastAckedTick}");
            GUI.Box(new Rect(5f, 125f, 180f, 25f), $"PREDICTED TICK {_clientTick}");
        }
    }
}