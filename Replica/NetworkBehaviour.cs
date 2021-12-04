using System;
using System.Collections.Generic;

namespace Replica
{
    public abstract class NetworkBehaviour
    {
        public long Flags { get; set; }

        public static bool NetVarEqual<T>(T value, ref T fieldValue) => EqualityComparer<T>.Default.Equals(value, fieldValue);


        //TODO replace with ref call to backing field on codegen maybe?
        public static bool NetVarEqual<T>(T value, T propertyValue) => EqualityComparer<T>.Default.Equals(value, propertyValue);


        public void SetNetVar<T>(T value, ref T fieldValue, long flag)
        {
            fieldValue = value;
			SetFlag(flag);
		}

		public bool GetNetVarGuard(long flag) => (Flags & flag) > 0L;

		public void SetNetVarGuard(long flag, bool value)
		{
			if (value)
			{
				Flags |= flag;
				return;
			}

			Flags &= ~flag;
		}

        public void SetFlag(long flag) => Flags |= flag;

        public virtual bool WriteNetVars(ref Packet writer, bool initial)
        {
            return false;
        }

        public virtual void ReadNetVars(ref Packet reader, bool initial)
        {

        }

    }
}
