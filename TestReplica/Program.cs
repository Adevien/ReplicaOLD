using Replica;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TestReplica
{
    class Program
    {
        static void Main(string[] args)
        {
            DemoEntity clientA = new DemoEntity("CLIENT_A");
            DemoEntity clientB = new DemoEntity("CLIENT_B");

            clientA.IsLocal = false;
            clientB.IsLocal = true;

            Console.WriteLine("----- INITIAL");

            clientA.Speed = 5.5f;

            NetBuffer buffer = new NetBuffer();

            clientA.WriteNetVars(buffer, true);
            clientB.ReadNetVars(buffer, true);

            Console.WriteLine($"CLIENT_A FLAG COUNT {clientA.Flags}");
            Console.WriteLine($"CLIENT_B FLAG COUNT {clientB.Flags}");

            for (int i = 0; i < 100; i++)
            {
                Console.WriteLine($"----- NORMAL STEP {i}");
                clientA.Speed = 12.5f * i;

                clientA.WriteNetVars(buffer, false);
                clientB.ReadNetVars(buffer, false);

                Console.WriteLine($"CLIENT_A FLAG COUNT {clientA.Flags}");
                Console.WriteLine($"CLIENT_B FLAG COUNT {clientB.Flags}");
            }

            Console.WriteLine("DONE");

            Console.ReadLine();
        }
    }
}
