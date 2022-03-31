using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Replica {
    public abstract class SimulationBehaviour : MonoBehaviour {
        private void OnEnable() => NetRunner.Subscribe(this);

        private void OnDisable() => NetRunner.Unsubscribe(this);

        //public virtual void PreUpdate() { }

        public virtual void OnFixedStep() { }

        public virtual void Render() { }
    }
}
