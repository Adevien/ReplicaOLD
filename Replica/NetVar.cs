using System;

namespace Replica
{
    [AttributeUsage(AttributeTargets.Property)]
    public class NetVar : Attribute
    {
        /// <summary>
        /// Event for property changed callback
        /// </summary>
        public string callback;

        /// <summary>
        /// Constructor for property changed callback
        /// </summary>
        /// <param name="Callback"></param>
        public NetVar(string _callback) => callback = _callback;

        /// <summary>
        /// Default constructor without property changed callback
        /// </summary>
        public NetVar()
        {

        }
    }
}
