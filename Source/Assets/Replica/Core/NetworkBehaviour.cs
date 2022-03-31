using Replica.Structures;
using System.Collections.Generic;

namespace Replica {
    public abstract class NetworkBehaviour : SimulationBehaviour {
        /// <summary>
        /// Check if its a local behaviour and not remote
        /// </summary>
        public bool IsLocal { get; set; }

        /// <summary>
        /// The bitmask for property flags
        /// </summary>
        public int Flags { get; set; }

        /// <summary>
        /// The bitmask for property guard flags
        /// </summary>
        public int Guards { get; set; }

        /// <summary>
        /// Checks the field and value equality to detect changes
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public static bool Equals<T>(T value, ref T field) {
            return EqualityComparer<T>.Default.Equals(value, field);
        }

        /// <summary>
        /// Sets the field value with the property value and sets the flag
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="field"></param>
        /// <param name="flag"></param>
        public void Set<T>(T value, ref T field, int flag) {
            Flags |= flag;
            field = value;
        }

        /// <summary>
        /// Checks if the property is guarded to avoid on changed callback
        /// </summary>
        /// <param name="flag"></param>
        /// <returns></returns>
        protected bool GetGuard(int flag) {
            return (Guards & flag) > 0;
        }

        /// <summary>
        /// Sets the bitmask guard state
        /// </summary>
        /// <param name="flag"></param>
        /// <param name="value"></param>
		protected void Guard(int flag, bool value) {
            if (value) {
                Guards |= flag;
                return;
            }

            Guards &= ~flag;
        }

        /// <summary>
        /// Writes all the changed fields and the flag to writer
        /// Has the toggle for initial state vs delta state
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="initial"></param>
        /// <returns></returns>
        public virtual bool WriteNetVars(Byter writer, bool initial) {
            return false;
        }

        /// <summary>
        /// Reads all the changed fields from the reader
        /// Has the toggle for initial state vs delta state
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="initial"></param>
        public virtual void ReadNetVars(Byter reader, bool initial) {

        }

        /// <summary>
        /// Returns the simulated inputs assigned to this instance
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public bool GetInput(out NetworkInput input) {
            input = default;
            return false;
        }
    }
}
