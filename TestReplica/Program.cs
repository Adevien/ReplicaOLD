using Replica;
using System;
using System.Collections.Generic;

namespace TestReplica
{
    class Program
    {
        static void Main(string[] args)
        {
            DemoEntity clientA = new DemoEntity(false, "CLIENT_A");

            DemoEntity clientB = new DemoEntity(true, "CLIENT_B");

            Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine("INITIAL RUN");

            //clientA.Health = 55;
            clientA.NetworkSpeed = 12.5f;

            NetBuffer bufferAX = new NetBuffer();

            clientA.WriteNetVars(bufferAX, true);
            clientB.ReadNetVars(bufferAX, true);

            Console.WriteLine($"CLIENT_A CHANGED FLAGS {clientA.Flags}");
            Console.WriteLine($"CLIENT_B CHANGED FLAGS {clientB.Flags}");

            Console.ForegroundColor = ConsoleColor.Green;

            for (int i = 1; i < 5; i++)
            {
                NetBuffer bufferA = new NetBuffer();

                Console.WriteLine($"NORMAL RUN {i} -->");

                clientA.NetworkSpeed = 1.5f * i;

                clientA.WriteNetVars(bufferA, false);
                clientB.ReadNetVars(bufferA, false);

                Console.WriteLine($"CLIENT_A CHANGED FLAGS {clientA.Flags}");
                Console.WriteLine($"CLIENT_B CHANGED FLAGS {clientB.Flags}");

            }


            Console.ReadLine();

        }
    }
}
