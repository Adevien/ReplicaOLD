using System;

namespace Replica {
    [Serializable]
    public class NetVar : Attribute {
        /// <summary>
        /// Event for property changed callback
        /// </summary>
        public string _callback;

        /// <summary>
        /// Constructor for property changed callback
        /// </summary>
        /// <param name="Callback"></param>
        public NetVar(string callback) => _callback = callback;

        /// <summary>
        /// Default constructor without property changed callback
        /// </summary>
        public NetVar() {

        }
    }
}
