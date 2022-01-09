using System;
using System.Collections.Generic;
using System.Text;

namespace Replica.Runtime
{
	public sealed class NetBuffer
	{
		private static int FindHighestBitPosition(byte data)
		{
			int num = 0;
			while (data > 0)
			{
				data = (byte)(data >> 1);
				num++;
			}
			return num;
		}

		private static byte ToASCII(char character)
		{
			byte b = 0;
			try
			{
				b = Convert.ToByte(character);
			}
			catch (OverflowException)
			{
				return 0;
			}
			if (b > 127)
			{
				return 0;
			}
			return b;
		}

		public int SizeInBytes
		{
			get
			{
				return (this.writePos - 1 >> 3) + 1;
			}
		}

		public bool IsDone
		{
			get
			{
				return this.writePos == this.readPos;
			}
		}

		public NetBuffer(int capacity = 8)
		{
			this.chunks = new uint[capacity];
			this.readPos = 0;
			this.writePos = 0;
		}

		public void Clear()
		{
			this.readPos = 0;
			this.writePos = 0;
		}

		public void CopyFromBuffer(NetBuffer buffer)
		{
			int num = buffer.readPos;
			int num2 = buffer.writePos % 8;
			if (buffer.writePos >= 8)
			{
				int num3 = buffer.writePos / 8;
				for (int i = 0; i < num3; i++)
				{
					this.WriteByte(buffer.ReadByte());
				}
			}
			if (num2 > 0)
			{
				this.WriteBits(num2, buffer.ReadBits(num2));
			}
			buffer.readPos = num;
		}

		public void WriteShort(short value)
		{
			this.WriteInt(NetBuffer.ShortCompressor, (int)value);
		}

		public void WriteUShort(ushort value)
		{
			this.WriteBits(16, (uint)value);
		}

		public void WriteFloat(float value)
		{
			this.WriteUInt32(new UIntFloat
			{
				FloatValue = value
			}.UIntValue);
		}

		public void WriteDouble(double value)
		{
			this.WriteUInt64(new UIntFloat
			{
				DoubleValue = value
			}.ULongValue);
		}

		public void WriteEnumAsShort(ValueType value)
		{
			this.WriteShort((short)value);
		}

		public short ReadShort()
		{
			return (short)this.ReadInt(NetBuffer.ShortCompressor);
		}

		public ushort ReadUShort()
		{
			return (ushort)this.ReadBits(16);
		}

		public float ReadFloat()
		{
			return new UIntFloat
			{
				UIntValue = this.ReadUInt32()
			}.FloatValue;
		}

		public double ReadDouble()
		{
			return new UIntFloat
			{
				ULongValue = this.ReadUInt64()
			}.DoubleValue;
		}

		public ValueType ReadEnumAsShort()
		{
			return this.ReadShort();
		}

		public void WriteBoolArray(bool[] array)
		{
			this.PackAll<bool>(array, delegate (bool element)
			{
				this.WriteBool(element);
			});
		}

		public void WriteByteArray(byte[] array)
		{
			this.PackAll<byte>(array, delegate (byte element)
			{
				this.WriteByte(element);
			});
		}

		public void WriteIntArray(int[] array)
		{
			this.PackAll<int>(array, delegate (int element)
			{
				this.WriteInt32(element);
			});
		}

		public void WriteInt64Array(long[] array)
		{
			this.PackAll<long>(array, delegate (long element)
			{
				this.WriteInt64(element);
			});
		}

		public void WriteUIntArray(uint[] array)
		{
			this.PackAll<uint>(array, delegate (uint element)
			{
				this.WriteUInt32(element);
			});
		}

		public void WriteUInt64Array(ulong[] array)
		{
			this.PackAll<ulong>(array, delegate (ulong element)
			{
				this.WriteUInt64(element);
			});
		}

		public void WriteShortArray(short[] array)
		{
			this.PackAll<short>(array, delegate (short element)
			{
				this.WriteShort(element);
			});
		}

		public void WriteUShortArray(ushort[] array)
		{
			this.PackAll<ushort>(array, delegate (ushort element)
			{
				this.WriteUShort(element);
			});
		}

		public void WriteFloatArray(float[] array)
		{
			this.PackAll<float>(array, delegate (float element)
			{
				this.WriteFloat(element);
			});
		}

		public void WriteDoubleArray(double[] array)
		{
			this.PackAll<double>(array, delegate (double element)
			{
				this.WriteDouble(element);
			});
		}

		public bool[] ReadBoolArray()
		{
			byte b = this.ReadByte();
			bool[] array = new bool[(int)b];
			array = ((b > 0) ? new bool[(int)b] : null);
			for (int i = 0; i < (int)b; i++)
			{
				array[i] = this.ReadBool();
			}
			return array;
		}

		public byte[] ReadByteArray()
		{
			byte b = this.ReadByte();
			byte[] array = new byte[(int)b];
			array = ((b > 0) ? new byte[(int)b] : null);
			for (int i = 0; i < (int)b; i++)
			{
				array[i] = this.ReadByte();
			}
			return array;
		}

		public int[] ReadIntArray()
		{
			byte b = this.ReadByte();
			int[] array = new int[(int)b];
			array = ((b > 0) ? new int[(int)b] : null);
			for (int i = 0; i < (int)b; i++)
			{
				array[i] = this.ReadInt32();
			}
			return array;
		}

		public long[] ReadInt64Array()
		{
			byte b = this.ReadByte();
			long[] array = new long[(int)b];
			array = ((b > 0) ? new long[(int)b] : null);
			for (int i = 0; i < (int)b; i++)
			{
				array[i] = this.ReadInt64();
			}
			return array;
		}

		public uint[] ReadUIntArray()
		{
			byte b = this.ReadByte();
			uint[] array = new uint[(int)b];
			array = ((b > 0) ? new uint[(int)b] : null);
			for (int i = 0; i < (int)b; i++)
			{
				array[i] = this.ReadUInt32();
			}
			return array;
		}

		public ulong[] ReadUInt64Array()
		{
			byte b = this.ReadByte();
			ulong[] array = new ulong[(int)b];
			array = ((b > 0) ? new ulong[(int)b] : null);
			for (int i = 0; i < (int)b; i++)
			{
				array[i] = this.ReadUInt64();
			}
			return array;
		}

		public short[] ReadShortArray()
		{
			byte b = this.ReadByte();
			short[] array = new short[(int)b];
			array = ((b > 0) ? new short[(int)b] : null);
			for (int i = 0; i < (int)b; i++)
			{
				array[i] = this.ReadShort();
			}
			return array;
		}

		public ushort[] ReadUShortArray()
		{
			byte b = this.ReadByte();
			ushort[] array = new ushort[(int)b];
			array = ((b > 0) ? new ushort[(int)b] : null);
			for (int i = 0; i < (int)b; i++)
			{
				array[i] = this.ReadUShort();
			}
			return array;
		}

		public float[] ReadFloatArray()
		{
			byte b = this.ReadByte();
			float[] array = new float[(int)b];
			array = ((b > 0) ? new float[(int)b] : null);
			for (int i = 0; i < (int)b; i++)
			{
				array[i] = this.ReadFloat();
			}
			return array;
		}

		public double[] ReadDoubleArray()
		{
			byte b = this.ReadByte();
			double[] array = new double[(int)b];
			array = ((b > 0) ? new double[(int)b] : null);
			for (int i = 0; i < (int)b; i++)
			{
				array[i] = this.ReadDouble();
			}
			return array;
		}

		public void WriteBits(int numBits, ulong value)
		{
			if (numBits <= 32)
			{
				this.WriteBits(numBits, (uint)value);
				return;
			}
			if (numBits > 64)
			{
				throw new ArgumentOutOfRangeException("Pushing too many bits");
			}
			this.WriteBits(32, (uint)value);
			int numBits2 = numBits - 32;
			uint value2 = (uint)(value >> 32);
			this.WriteBits(numBits2, value2);
		}

		public void WriteBits(int numBits, uint value)
		{
			if (numBits < 0)
			{
				throw new ArgumentOutOfRangeException("Pushing negatve bits");
			}
			if (numBits > 32)
			{
				throw new ArgumentOutOfRangeException("Pushing too many bits");
			}
			int num = this.writePos >> 5;
			int num2 = this.writePos & 31;
			if (num + 1 >= this.chunks.Length)
			{
				this.ExpandArray();
			}
			ulong num3 = (1UL << num2) - 1UL;
			ulong num4 = ((ulong)this.chunks[num] & num3) | (ulong)value << num2;
			this.chunks[num] = (uint)num4;
			this.chunks[num + 1] = (uint)(num4 >> 32);
			this.writePos += numBits;
		}

		public ulong ReadBitsULong(int numBits)
		{
			if (numBits <= 32)
			{
				ulong result = (ulong)this.Peek(numBits);
				this.readPos += numBits;
				return result;
			}
			if (numBits > 64)
			{
				throw new ArgumentOutOfRangeException("Pushing too many bits");
			}
			ulong num = (ulong)this.Peek(32);
			this.readPos += 32;
			int num2 = numBits - 32;
			ulong num3 = (ulong)this.Peek(num2);
			this.readPos += num2;
			num3 <<= 32;
			return num | num3;
		}

		public uint ReadBits(int numBits)
		{
			uint result = this.Peek(numBits);
			this.readPos += numBits;
			return result;
		}

		public uint Peek(int numBits)
		{
			if (numBits < 0)
			{
				throw new ArgumentOutOfRangeException("Pushing negatve bits");
			}
			if (numBits > 32)
			{
				throw new ArgumentOutOfRangeException("Pushing too many bits");
			}
			int num = this.readPos >> 5;
			int num2 = this.readPos & 31;
			ulong num3 = (1UL << numBits) - 1UL << num2;
			ulong num4 = (ulong)this.chunks[num];
			if (num + 1 < this.chunks.Length)
			{
				num4 |= (ulong)this.chunks[num + 1] << 32;
			}
			return (uint)((num4 & num3) >> num2);
		}

		public int Store(byte[] data)
		{
			this.WriteBits(1, 1U);
			int num = (this.writePos >> 5) + 1;
			for (int i = 0; i < num; i++)
			{
				int num2 = i * 4;
				uint num3 = this.chunks[i];
				data[num2] = (byte)num3;
				data[num2 + 1] = (byte)(num3 >> 8);
				data[num2 + 2] = (byte)(num3 >> 16);
				data[num2 + 3] = (byte)(num3 >> 24);
			}
			return this.SizeInBytes;
		}

		public void Load(byte[] data, int length)
		{
			int num = length / 4 + 1;
			if (this.chunks.Length < num)
			{
				this.chunks = new uint[num];
			}
			for (int i = 0; i < num; i++)
			{
				int num2 = i * 4;
				uint num3 = (uint)((int)data[num2] | (int)data[num2 + 1] << 8 | (int)data[num2 + 2] << 16 | (int)data[num2 + 3] << 24);
				this.chunks[i] = num3;
			}
			int num4 = NetBuffer.FindHighestBitPosition(data[length - 1]);
			this.writePos = (length - 1) * 8 + (num4 - 1);
			this.readPos = 0;
		}

		internal void Insert(int position, int numBits, uint value)
		{
			if (numBits < 0)
			{
				throw new ArgumentOutOfRangeException("Pushing negatve bits");
			}
			if (numBits > 32)
			{
				throw new ArgumentOutOfRangeException("Pushing too many bits");
			}
			int num = position >> 5;
			int num2 = position & 31;
			ulong num3 = (1UL << numBits) - 1UL;
			ulong num4 = ((ulong)value & num3) << num2;
			ulong num5 = (ulong)this.chunks[num] | (ulong)this.chunks[num + 1] << 32 | num4;
			this.chunks[num] = (uint)num5;
			this.chunks[num + 1] = (uint)(num5 >> 32);
		}

		private void ExpandArray()
		{
			uint[] destinationArray = new uint[this.chunks.Length * 2 + 1];
			Array.Copy(this.chunks, destinationArray, this.chunks.Length);
			this.chunks = destinationArray;
		}

		public byte PackAll<T>(IEnumerable<T> elements, Action<T> encode)
		{
			byte b = 0;
			int position = this.writePos;
			this.WriteBits(8, 0U);
			foreach (T obj in elements)
			{
				if (b == 255)
				{
					break;
				}
				encode(obj);
				b += 1;
			}
			this.Insert(position, 8, (uint)b);
			return b;
		}

		public byte PackToSize<T>(int maxTotalBytes, int maxIndividualBytes, IEnumerable<T> elements, Action<T> encode, Action<T> packed = null)
		{
			maxTotalBytes--;
			byte b = 0;
			int position = this.writePos;
			this.WriteBits(8, 0U);
			foreach (T obj in elements)
			{
				if (b == 255)
				{
					break;
				}
				int num = this.writePos;
				int sizeInBytes = this.SizeInBytes;
				encode(obj);
				int sizeInBytes2 = this.SizeInBytes;
				if (sizeInBytes2 - sizeInBytes > maxIndividualBytes)
				{
					this.writePos = num;
				}
				else
				{
					if (sizeInBytes2 > maxTotalBytes)
					{
						this.writePos = num;
						break;
					}
					if (packed != null)
					{
						packed(obj);
					}
					b += 1;
				}
			}
			this.Insert(position, 8, (uint)b);
			return b;
		}

		public IEnumerable<T> UnpackAll<T>(Func<T> decode)
		{
			byte count = this.ReadByte();
			uint num;
			for (uint i = 0U; i < (uint)count; i = num + 1U)
			{
				yield return decode();
				num = i;
			}
			yield break;
		}

		public void WriteByte(byte val)
		{
			this.WriteBits(8, (uint)val);
		}

		public byte ReadByte()
		{
			return (byte)this.ReadBits(8);
		}

		public byte PeekByte()
		{
			return (byte)this.Peek(8);
		}

		public void WriteUInt32(uint value)
		{
			this.WriteBits(32, value);
		}

		public void WriteUInt64(ulong value)
		{
			this.WriteBits(64, value);
		}

		public void WriteInt32(int value)
		{
			this.WriteBits(32, (uint)value);
		}

		public void WriteInt(int value)
		{
			this.WriteInt32(value);
		}

		public void WriteInt64(long value)
		{
			this.WriteBits(64, (ulong)value);
		}

		public uint ReadUInt32()
		{
			return this.ReadBits(32);
		}

		public ulong ReadUInt64()
		{
			return this.ReadBitsULong(64);
		}

		public int ReadInt32()
		{
			return (int)this.ReadBits(32);
		}

		public int ReadInt()
		{
			return this.ReadInt32();
		}

		public long ReadInt64()
		{
			return (long)this.ReadBitsULong(64);
		}

		internal void WriteVarUInt(uint val)
		{
			do
			{
				uint num = val & 127U;
				val >>= 7;
				if (val > 0U)
				{
					num |= 128U;
				}
				this.WriteBits(8, num);
			}
			while (val > 0U);
		}

		internal uint ReadVarUInt()
		{
			uint num = 0U;
			int num2 = 0;
			uint num3;
			do
			{
				num3 = this.ReadBits(8);
				num |= (num3 & 127U) << num2;
				num2 += 7;
			}
			while ((num3 & 128U) > 0U);
			return num;
		}

		public uint PeekVarUInt()
		{
			int num = this.readPos;
			uint result = this.ReadVarUInt();
			this.readPos = num;
			return result;
		}

		public void WriteVarInt(int val)
		{
			uint val2 = (uint)(val << 1 ^ val >> 31);
			this.WriteVarUInt(val2);
		}

		public void WriteVarInt64(long val)
		{
			ulong value = (ulong)(val << 1 ^ val >> 63);
			this.WriteUInt64(value);
		}

		public int ReadVarInt()
		{
			uint num = this.ReadVarUInt();
			return -((int)((ulong)(num >> 1) ^ (ulong)(num & 1U)));

		}

		public long ReadVarInt64()
		{
			ulong num = this.ReadUInt64();
			return (long)((num >> 1 & 9223372036854775807UL) ^ num << 63 >> 63);
		}

		public int PeekVarInt()
		{
			uint num = this.PeekVarUInt();
			return -((int)((ulong)(num >> 1) ^ (ulong)(num & 1U)));
		}

		public void WriteBool(bool value)
		{
			this.WriteBits(1, value ? 1U : 0U);
		}

		public bool ReadBool()
		{
			return this.ReadBits(1) > 0U;
		}

		public bool PeekBool()
		{
			return this.Peek(1) > 0U;
		}

		public void WriteString(string value)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			uint num = (uint)value.Length;
			if (value.Length > 63)
			{
				num = 63U;
			}
			this.WriteBits(NetBuffer.STRING_LENGTH_BITS, num);
			int num2 = 0;
			while ((long)num2 < (long)((ulong)num))
			{
				this.WriteBits(7, (uint)NetBuffer.ToASCII(value[num2]));
				num2++;
			}
		}

		public string ReadString()
		{
			StringBuilder stringBuilder = new StringBuilder("");
			uint num = this.ReadBits(NetBuffer.STRING_LENGTH_BITS);
			int num2 = 0;
			while ((long)num2 < (long)((ulong)num))
			{
				stringBuilder.Append((char)this.ReadBitsULong(7));
				num2++;
			}
			return stringBuilder.ToString();
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = this.chunks.Length - 1; i >= 0; i--)
			{
				stringBuilder.Append(Convert.ToString((long)((ulong)this.chunks[i]), 2).PadLeft(32, '0'));
			}
			StringBuilder stringBuilder2 = new StringBuilder();
			for (int j = 0; j < stringBuilder.Length; j++)
			{
				stringBuilder2.Append(stringBuilder[j]);
				if ((j + 1) % 8 == 0)
				{
					stringBuilder2.Append(" ");
				}
			}
			return stringBuilder2.ToString();
		}

		internal int readPos;

		internal int writePos;

		private uint[] chunks;

		private static IntCompressor ShortCompressor = new IntCompressor(0, 65535);

		internal static readonly FloatCompressor QuatComponentCompressor = new FloatCompressor(-256f, 255f, 1f);

		internal static readonly IntCompressor QuatIndexCompressor = new IntCompressor(0, 3);

		internal const float FLOAT_PRECISION_MULTI = 255f;

		private static readonly int STRING_LENGTH_BITS = Utils.FastLog2(63U);

	}
}
