using System;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Replica.Utils
{
    public unsafe class ByteBuffer
    {
        public bool HasData =>  Position < Size;

        public void Reset()
        {
            Position = 0;
            Size = 0;
        }

        public static ByteBuffer Allocate()
        {
            lock (Sync)
            {
                if (Pool != null)
                {
                    ByteBuffer instance = Pool;
                    Pool = Pool.Next;
                    return instance;
                }
            }

            return new ByteBuffer();
        }

        public static void Recycle(ByteBuffer buffer)
        {
            buffer.Reset();

            lock (Sync)
            {
                buffer.Next = Pool;
                Pool = buffer;
            }
        }

        public void Dispose()
        {
            if (Data == null) return;

            Marshal.FreeHGlobal((IntPtr)Data);
            Data = null;
            GC.SuppressFinalize(this);
        }

        ~ByteBuffer()
        {
            if (Data == null) return;

            Marshal.FreeHGlobal((IntPtr)Data);
            Data = null;
        }

        public int Position;
        public int Size;

        internal ByteBuffer Next;

        static readonly object Sync = new object();
        static ByteBuffer Pool;

        internal byte* Data;

        public const int Mtu = 1280;

        [StructLayout(LayoutKind.Explicit)]
        private struct ConverterHelper
        {
            [FieldOffset(0)]
            public int INT;

            [FieldOffset(0)]
            public float FLOAT;
        }

        public ByteBuffer()
        {
            Data = (byte*)Marshal.AllocHGlobal(Mtu);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Put(float value)
        {
            ConverterHelper ch = new ConverterHelper { FLOAT = value };
            Put(ch.INT);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Put(ulong value)
        {
            *(ulong*)(Data + Position) = value;
            Position += 8;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Put(int value)
        {
            *(int*)(Data + Position) = value;
            Position += 4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Put(uint value)
        {
            *(uint*)(Data + Position) = value;
            Position += 4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Put(ushort value)
        {
            *(ushort*)(Data + Position) = value;
            Position += 2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Put(short value)
        {
            *(short*)(Data + Position) = value;
            Position += 2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Put(sbyte value)
        {
            Data[Position] = (byte)value;
            Position++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Put(byte value)
        {
            Data[Position] = value;
            Position++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Put(bool value)
        {
            Data[Position] = (byte)(value ? 1 : 0);
            Position++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PutArray(string[] value)
        {
            ushort len = value == null ? (ushort)0 : (ushort)value.Length;

            Put(len);

            for (int i = 0; i < len; i++)
                Put(value[i]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PutArray(string[] value, int maxLength)
        {
            ushort len = value == null ? (ushort)0 : (ushort)value.Length;

            Put(len);

            for (int i = 0; i < len; i++)
                Put(value[i], maxLength);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PutArray(ushort[] value)
        {
            int len = value.Length;

            Put((ushort)len);

            for (int i = 0; i < len; i++)
                Put(value[i]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Put(IPEndPoint endPoint)
        {
            Put(endPoint.Address.ToString());
            Put(endPoint.Port);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Put(string value)
        {
            Put(value, 80);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Put(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
            {
                Put((ushort)0);
                return;
            }

            int length = value.Length > maxLength ? maxLength : value.Length;
            int bytesCount;
            int byteCountPosition = Position;

            Position += 2;

            fixed (char* str = value)
                bytesCount = Encoding.UTF8.GetBytes(str, length, Data + Position, maxLength * 2);

            *(ushort*)(Data + byteCountPosition) = (ushort)(bytesCount);

            Position += bytesCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetByte()
        {
            byte res = Data[Position];
            Position += 1;
            return res;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sbyte GetSByte()
        {
            var b = (sbyte)Data[Position];
            Position++;
            return b;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetBool()
        {
            bool res = Data[Position] > 0;
            Position += 1;
            return res;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort GetUShort()
        {
            ushort result;
            result = *(ushort*)(Data + Position);
            Position += 2;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short GetShort()
        {
            short result = *(short*)(Data + Position);
            Position += 2;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long GetLong()
        {
            long result = *(long*)(Data + Position);
            Position += 8;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong GetULong()
        {
            ulong result = *(ulong*)(Data + Position);
            Position += 8;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetInt()
        {
            int result = *(int*)(Data + Position);
            Position += 4;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetUInt()
        {
            uint result = *(uint*)(Data + Position);
            Position += 4;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetFloat()
        {
            ConverterHelper ch = new ConverterHelper { INT = GetInt() };
            return ch.FLOAT;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort[] GetUShortArray()
        {
            int len = GetUShort();
            ushort[] result = new ushort[len];

            for (int it = 0; it < len; ++it)
                result[it] = GetUShort();

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetString(int maxLength)
        {
            int bytesCount = GetUShort();

            if (bytesCount <= 0 || bytesCount > maxLength * 2)
                return string.Empty;

            int charCount;
            string result;

            charCount = Encoding.UTF8.GetCharCount(Data + Position, bytesCount);

            if (charCount > maxLength)
                return string.Empty;

            result = Encoding.UTF8.GetString(Data + this.Position, bytesCount);

            Position += bytesCount;

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetString()
        {
            return GetString(80);
        }
    }
}
