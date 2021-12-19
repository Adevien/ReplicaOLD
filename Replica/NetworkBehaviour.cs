using System;
using System.Collections.Generic;

namespace Replica
{
    public abstract class NetworkBehaviour
    {
        public int Flags { get; set; }

        public static bool NetVarEqual<T>(T value, ref T fieldValue) => EqualityComparer<T>.Default.Equals(value, fieldValue);

        public void SetNetVar<T>(T value, ref T fieldValue, int flag)
        {
			SetFlag(flag);
            fieldValue = value;
		}

		public bool GetLock(int flag) => (Flags & flag) > 0;

		public void SetLock(int flag, bool value)
		{
            if (value)
            {
                Flags |= flag;
                return;
            }

            Flags &= ~flag;
        }

        public void SetFlag(int flag) => Flags |= flag;

        public virtual bool WriteNetVars(NetBuffer writer, bool initial)
        {
            return false;
        }

        public virtual void ReadNetVars(NetBuffer reader, bool initial)
        {

        }

    }
}
