using System;
using Replica;

namespace TestReplica
{
	public class DemoEntity : NetworkBehaviour
	{
		[NetVar(nameof(OnValueChange))] public float Speed { get; set; }

		[NetVar] public float Health { get; set; }

		public void OnValueChange(float value)
		{
			Console.WriteLine($"{Name}: Value changed to {value}");
		}

		public string Name = "";

        public DemoEntity(string name)
        {
			Name = name;

		}
    }
}
