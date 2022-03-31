using Replica.Structures;
using Replica.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace Replica {

    public class NetRunner : MonoBehaviour {

        public static NetRunner Instance;

        private void Awake() {
            Instance = this;
        }

        public RefField<IInputEvent> OnInput;

        static List<SimulationBehaviour> _simulationBehaviours = new List<SimulationBehaviour>();

        public const int StepRate = 32;

        public const float StepTime = 1.0f / StepRate;

        double _stepAccumulator = 0.0;

        public static float Delta { get; private set; }

        public LatencySimulator LagSim;

        public NetworkInput _storedInputs;

        public NetworkInput _receivedInputs;

        public static void Subscribe(SimulationBehaviour sim) => _simulationBehaviours.Add(sim);

        public static void Unsubscribe(SimulationBehaviour sim) => _simulationBehaviours.Remove(sim);

        private void Update() {
            foreach (SimulationBehaviour behaviour in _simulationBehaviours) {
                behaviour.PreUpdate();

                _storedInputs.Reset();
                OnInput?.Value?.OnInput(ref _storedInputs);
            }

            _stepAccumulator += Time.deltaTime;

            while (_stepAccumulator >= StepTime) {
                _stepAccumulator -= StepTime;

                foreach (SimulationBehaviour behaviour in _simulationBehaviours)
                    behaviour.OnFixedStep();
            }

            Delta = Mathf.Clamp01((float)(_stepAccumulator / StepTime));

            foreach (SimulationBehaviour behaviour in _simulationBehaviours)
                behaviour.Render();
        }

        private void OnGUI() {
            GUI.Box(new Rect(5f, 05f, 180f, 25f), $"RTT SIMULATION {LagSim.Latency * 1000f}");
            GUI.Box(new Rect(5f, 35f, 180f, 25f), $"PACKET LOSS {LagSim.PacketLoss * 100f} %");
        }
    }
}
