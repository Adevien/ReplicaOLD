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
		public void OnValueChange(float value)
		{
			Console.WriteLine($"{Name}: Value changed to {value}");
		}

		[NetVar(nameof(OnValueChange))] public float Speed { get; set; }

		[NetVar] public float Health { get; set; }


		public string Name = "";

        public DemoEntity(string name)
        {
			Name = name;

		}
    }
}
