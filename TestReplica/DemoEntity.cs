using Replica;
using System;

namespace TestReplica
{
    public class DemoEntity : NetworkBehaviour
    {
        [NetVar(nameof(OnSpeedChanged))] public float Speed { get; set; }

        public void OnSpeedChanged(float newValue)
        {
            Console.WriteLine($"Propery changed to {newValue}");
        }

        //TODO: add codegen for the WriteNetVars method
        // currently has to be manually addded
        public override bool WriteNetVars(ref Packet writer, bool initial)
        {
            bool written = base.WriteNetVars(ref writer, initial);

            if (initial)
            {
                writer.Write(Speed);
                return true;
            }
            else
            {
                writer.Write(Flags);

                if ((Flags & 2L) != 0L)
                {
                    writer.Write(Speed);
                    written = true;
                }

                return written;
            }
        }

        //TODO: add codegen for the ReadNetVars method
        // currently has to be manually addded
        public override void ReadNetVars(ref Packet reader, bool initial)
        {
            base.ReadNetVars(ref reader, initial);

            if (initial)
            {
                float num = reader.ReadFloat();

                if (!NetVarEqual(num, Speed))
                    OnSpeedChanged(Speed);

                return;
            }

            long dirtyFlags = reader.ReadLong();

            if ((dirtyFlags & 2L) != 0)
            {
                float num = reader.ReadFloat();

                if (!NetVarEqual(num, Speed))
                    OnSpeedChanged(Speed);
            }
        }
    }
}
