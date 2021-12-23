using System;
using System.Collections.Generic;

namespace Replica
{
    public abstract class NetworkBehaviour
    {
        public int Flags { get; set; }

        public int FlagGuard { get; set; }

        public bool IsLocal { get; set; }

        public static bool Equals<T>(T value, ref T fieldValue) => EqualityComparer<T>.Default.Equals(value, fieldValue);

        public void Set<T>(T value, ref T fieldValue, int flag)
        {
			SetFlag(flag);
            fieldValue = value;
		}

		protected bool GetGuard(int flag)
		{
			return (this.FlagGuard & flag) > 0;
		}

		protected void Guard(int flag, bool value)
		{
			if (value)
			{
				this.FlagGuard |= flag;
				return;
			}
			this.FlagGuard &= ~flag;
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
