using System;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Replica.Runtime {
    public unsafe class Byter {
        internal Byter Next;

        private static Byter Pool;

        private static readonly object Sync = new object();

        public bool HasData => Position < Size;

        public int Position;

        private int Size;

        public byte* Data;

        public Byter(int size = 1280) {
            Data = (byte*)Marshal.AllocHGlobal(size);
        }

        public void Dispose() {
            if (Data != null) {
                Marshal.FreeHGlobal((IntPtr)Data);
                Data = null;
                GC.SuppressFinalize(this);
            }
        }

        ~Byter() {
            if (Data != null) {
                Marshal.FreeHGlobal((IntPtr)Data);
                Data = null;
            }
        }

        public static Byter Allocate() {
            lock (Sync) {
                if (Pool != null) {
                    Byter instance = Pool;
                    Pool = Pool.Next;
                    return instance;
                }
            }

            return new Byter();
        }

        public static void Recycle(Byter buffer) {
            buffer.Reset();

            lock (Sync) {
                buffer.Next = Pool;
                Pool = buffer;
            }
        }

        public void SetSize(int size) {
            Size = size;
        }

        public void Reset() {
            Position = 0;
            Size = 0;
        }

        [MethodImpl(256)]
        public void Put(int value) {
            *(int*)(Data + Position) = value;
            Position += 4;
        }

        [MethodImpl(256)]
        public void Put(uint value) {
            *(uint*)(Data + Position) = value;
            Position += 4;
        }

        [MethodImpl(256)]
        public void Put(ushort value) {
            *(ushort*)(Data + Position) = value;
            Position += 2;
        }

        [MethodImpl(256)]
        public void Put(short value) {
            *(short*)(Data + Position) = value;
            Position += 2;
        }

        [MethodImpl(256)]
        public void Put(sbyte value) {
            Data[Position] = (byte)value;
            Position++;
        }

        [MethodImpl(256)]
        public void Put(byte value) {
            Data[Position] = value;
            Position++;
        }

        [MethodImpl(256)]
        public void Put(bool value) {
            Data[Position] = (byte)(value ? 1 : 0);
            Position++;
        }

        [MethodImpl(256)]
        public void Put(float value) {
            *(float*)(Data + Position) = value;
            Position += 4;
        }

        [MethodImpl(256)]
        public void Put(double value) {
            *(double*)(Data + Position) = value;
            Position += 8;
        }

        [MethodImpl(256)]
        public void Put(long value) {
            *(long*)(Data + Position) = value;
            Position += 8;
        }

        [MethodImpl(256)]
        public void Put(ulong value) {
            *(ulong*)(Data + Position) = value;
            Position += 8;
        }

        [MethodImpl(256)]
        public void Put(string value) {
            Put(value, 256);
        }

        [MethodImpl(256)]
        public void Put(IPEndPoint endPoint) {
            Put(endPoint.Address.ToString());
            Put(endPoint.Port);
        }

        [MethodImpl(256)]
        public void PutArray(string[] value) {
            ushort len = value == null ? (ushort)0 : (ushort)value.Length;

            Put(len);

            for (int i = 0; i < len; i++)
                Put(value[i]);
        }

        [MethodImpl(256)]
        public void PutArray(string[] value, int maxLength) {
            ushort len = value == null ? (ushort)0 : (ushort)value.Length;

            Put(len);

            for (int i = 0; i < len; i++)
                Put(value[i], maxLength);
        }

        [MethodImpl(256)]
        public void PutArray(ushort[] value) {
            int len = value.Length;

            Put((ushort)len);

            for (int i = 0; i < len; i++)
                Put(value[i]);
        }

        [MethodImpl(256)]
        public void PutArray<T>(T[] value) where T : unmanaged {
            ushort len = (ushort)(value?.Length ?? 0);

            Put(len);

            fixed (T* ptr = &value[0])
                NativeUtils.MemCpyFast(Data, ptr, value.Length * sizeof(T));
        }

        [MethodImpl(256)]
        public void Put(string value, int maxLength) {
            if (string.IsNullOrEmpty(value)) {
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

        [MethodImpl(256)]
        public byte GetByte() {
            byte result = Data[Position];
            Position++;
            return result;
        }

        [MethodImpl(256)]
        public sbyte GetSByte() {
            sbyte result = (sbyte)Data[Position];
            Position++;
            return result;
        }

        [MethodImpl(256)]
        public bool GetBoolean() {
            bool result = Data[Position] > 0;
            Position += 1;
            return result;
        }

        [MethodImpl(256)]
        public short GetInt16() {
            short result = *(short*)(Data + Position);
            Position += 2;
            return result;
        }

        [MethodImpl(256)]
        public ushort GetUInt16() {
            ushort result = *(ushort*)(Data + Position);
            Position += 2;
            return result;
        }

        [MethodImpl(256)]
        public int GetInt32() {
            int result = *(int*)(Data + Position);
            Position += 4;
            return result;
        }

        [MethodImpl(256)]
        public uint GetUInt32() {
            uint result = *(uint*)(Data + Position);
            Position += 4;
            return result;
        }

        [MethodImpl(256)]
        public long GetInt64() {
            long result = *(long*)(Data + Position);
            Position += 8;
            return result;
        }

        [MethodImpl(256)]
        public ulong GetUInt64() {
            ulong result = *(ulong*)(Data + Position);
            Position += 8;
            return result;
        }

        [MethodImpl(256)]
        public float GetSingle() {
            float result = *(float*)(Data + Position);
            Position += 4;
            return result;
        }

        [MethodImpl(256)]
        public double GetDouble() {
            double result = *(double*)(Data + Position);
            Position += 8;
            return result;
        }

        [MethodImpl(256)]
        public string GetString() {
            return GetStringData(80);
        }

        [MethodImpl(256)]
        public ushort[] GetUInt16Array() {
            int len = GetUInt16();
            ushort[] result = new ushort[len];

            for (int it = 0; it < len; ++it)
                result[it] = GetUInt16();

            return result;
        }

        [MethodImpl(256)]
        public string GetStringData(int maxLength) {
            int bytesCount = GetUInt16();

            if (bytesCount <= 0 || bytesCount > maxLength * 2)
                return string.Empty;

            int charCount;
            string result;

            charCount = Encoding.UTF8.GetCharCount(Data + Position, bytesCount);

            if (charCount > maxLength)
                return string.Empty;

            result = Encoding.UTF8.GetString(Data + Position, bytesCount);

            Position += bytesCount;

            return result;
        }
    }
}
