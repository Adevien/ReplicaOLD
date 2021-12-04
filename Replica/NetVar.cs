using System;

namespace Replica
{
    [AttributeUsage(AttributeTargets.Property)]
    public class NetVar : Attribute
    {
        /// <summary>
        /// Method for property changed callback;
        /// </summary>
        public string Event;

        /// <summary>
        /// Passes a method name for property changed callback
        /// </summary>
        /// <param name="Callback"></param>
        public NetVar(string Callback) => Event = Callback;

        /// <summary>
        /// Default constructor without property changed callback
        /// </summary>
        public NetVar()
        {

        }
    }
}
