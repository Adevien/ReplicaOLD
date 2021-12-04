using System;

namespace TestReplica
{
    class Program
	{
		static void Main(string[] args)
        {
            DemoEntity server = new DemoEntity();

            DemoEntity client = new DemoEntity();

            //stolen packet class from Tom cause i'm lazy to make one rn.
            Packet packet = new Packet();

            Console.WriteLine($"CHANGED FLAGS {server.Flags}");

            server.Speed = 5.5f;

            server.WriteNetVars(ref packet, true);

            Console.WriteLine($"CHANGED FLAGS {client.Flags}");

            Console.WriteLine(packet.ToArray().Length);

            client.ReadNetVars(ref packet, true);

            Console.ReadLine();
        }
    }
}
