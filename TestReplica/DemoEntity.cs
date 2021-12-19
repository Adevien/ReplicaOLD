using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Replica;

namespace TestReplica
{
	// Token: 0x02000002 RID: 2
	public class DemoEntity : NetworkBehaviour
	{
		public void OnValueChange(float old, float value)
		{
			Console.WriteLine($"{Name}: Value changed to " + value.ToString());
		}

		public float NetworkSpeed
		{
			get
			{
				return this.Speed;
			}
			[param: In]
			set
			{
				if (!NetworkBehaviour.NetVarEqual<float>(value, ref this.Speed))
				{
					float speed = this.Speed;
					base.SetNetVar<float>(value, ref this.Speed, 2);
					if (IsLocal && !base.GetLock(2))
					{
						base.SetLock(2, true);
						this.OnValueChange(speed, value);
						base.SetLock(2, false);
					}
				}
			}
		}
		public override bool WriteNetVars(NetBuffer writer, bool forceAll)
		{
			bool result = base.WriteNetVars(writer, forceAll);
			if (forceAll)
			{
				writer.WriteFloat(this.Speed);
				return true;
			}
			writer.WriteInt32(base.Flags);

			if ((base.Flags & 2) != 0)
			{
				writer.WriteFloat(this.Speed);
				result = true;
			}
			return result;
		}

		public override void ReadNetVars(NetBuffer reader, bool initialState)
		{
			base.ReadNetVars(reader, initialState);
			if (initialState)
			{
				float speed = this.Speed;
				this.NetworkSpeed = reader.ReadFloat();
				if (!NetworkBehaviour.NetVarEqual<float>(speed, ref this.Speed))
				{
					this.OnValueChange(speed, this.Speed);
				}
				return;
			}

			int num = reader.ReadInt32();

			if ((num & 2) != 0)
			{
				float speed2 = this.Speed;
				this.NetworkSpeed = reader.ReadFloat();
				if (!NetworkBehaviour.NetVarEqual<float>(speed2, ref this.Speed))
				{
					this.OnValueChange(speed2, this.Speed);
				}
			}
		}

		public float Speed;

		public bool IsLocal = false;

		public string Name = "";

        public DemoEntity(bool isLocal, string name)
        {
			IsLocal = isLocal;
			Name = name;

		}
    }
}
