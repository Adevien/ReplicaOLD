using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using static Replica.NetBuffer;
using static Replica.Utils;

namespace Replica
{
	// Token: 0x02000096 RID: 150
	public sealed class NetBuffer
	{
		// Token: 0x060004AC RID: 1196 RVA: 0x0000E60C File Offset: 0x0000C80C
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

		// Token: 0x060004AD RID: 1197 RVA: 0x0000E62C File Offset: 0x0000C82C
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

		// Token: 0x170000DD RID: 221
		// (get) Token: 0x060004AE RID: 1198 RVA: 0x0000E664 File Offset: 0x0000C864
		public int SizeInBytes
		{
			get
			{
				return (this.writePos - 1 >> 3) + 1;
			}
		}

		// Token: 0x170000DE RID: 222
		// (get) Token: 0x060004AF RID: 1199 RVA: 0x0000E672 File Offset: 0x0000C872
		public bool IsDone
		{
			get
			{
				return this.writePos == this.readPos;
			}
		}

		// Token: 0x060004B0 RID: 1200 RVA: 0x0000E682 File Offset: 0x0000C882
		public NetBuffer(int capacity = 8)
		{
			this.chunks = new uint[capacity];
			this.readPos = 0;
			this.writePos = 0;
		}

		// Token: 0x060004B1 RID: 1201 RVA: 0x0000E6A4 File Offset: 0x0000C8A4
		public void Clear()
		{
			this.readPos = 0;
			this.writePos = 0;
		}

		// Token: 0x060004B2 RID: 1202 RVA: 0x0000E6B4 File Offset: 0x0000C8B4
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

		// Token: 0x060004B3 RID: 1203 RVA: 0x0000E714 File Offset: 0x0000C914
		public void WriteShort(short value)
		{
			this.WriteInt(NetBuffer.ShortCompressor, (int)value);
		}

		// Token: 0x060004B4 RID: 1204 RVA: 0x0000E722 File Offset: 0x0000C922
		public void WriteUShort(ushort value)
		{
			this.WriteBits(16, (uint)value);
		}

		// Token: 0x060004B5 RID: 1205 RVA: 0x0000E730 File Offset: 0x0000C930
		public void WriteFloat(float value)
		{
			this.WriteUInt32(new UIntFloat
			{
				FloatValue = value
			}.UIntValue);
		}

		// Token: 0x060004B6 RID: 1206 RVA: 0x0000E75C File Offset: 0x0000C95C
		public void WriteDouble(double value)
		{
			this.WriteUInt64(new UIntFloat
			{
				DoubleValue = value
			}.ULongValue);
		}

		// Token: 0x060004B7 RID: 1207 RVA: 0x0000E785 File Offset: 0x0000C985
		public void WriteEnumAsShort(ValueType value)
		{
			this.WriteShort((short)value);
		}

		// Token: 0x060004B8 RID: 1208 RVA: 0x0000E793 File Offset: 0x0000C993
		public short ReadShort()
		{
			return (short)this.ReadInt(NetBuffer.ShortCompressor);
		}

		// Token: 0x060004B9 RID: 1209 RVA: 0x0000E7A1 File Offset: 0x0000C9A1
		public ushort ReadUShort()
		{
			return (ushort)this.ReadBits(16);
		}

		// Token: 0x060004BA RID: 1210 RVA: 0x0000E7AC File Offset: 0x0000C9AC
		public float ReadFloat()
		{
			return new UIntFloat
			{
				UIntValue = this.ReadUInt32()
			}.FloatValue;
		}

		// Token: 0x060004BB RID: 1211 RVA: 0x0000E7D4 File Offset: 0x0000C9D4
		public double ReadDouble()
		{
			return new UIntFloat
			{
				ULongValue = this.ReadUInt64()
			}.DoubleValue;
		}

		// Token: 0x060004BC RID: 1212 RVA: 0x0000E7FC File Offset: 0x0000C9FC
		public ValueType ReadEnumAsShort()
		{
			return this.ReadShort();
		}

		// Token: 0x060004C1 RID: 1217 RVA: 0x0000E82F File Offset: 0x0000CA2F
		public void WriteBoolArray(bool[] array)
		{
			this.PackAll<bool>(array, delegate (bool element)
			{
				this.WriteBool(element);
			});
		}

		// Token: 0x060004C2 RID: 1218 RVA: 0x0000E845 File Offset: 0x0000CA45
		public void WriteByteArray(byte[] array)
		{
			this.PackAll<byte>(array, delegate (byte element)
			{
				this.WriteByte(element);
			});
		}

		// Token: 0x060004C3 RID: 1219 RVA: 0x0000E85B File Offset: 0x0000CA5B
		public void WriteIntArray(int[] array)
		{
			this.PackAll<int>(array, delegate (int element)
			{
				this.WriteInt32(element);
			});
		}

		// Token: 0x060004C4 RID: 1220 RVA: 0x0000E871 File Offset: 0x0000CA71
		public void WriteInt64Array(long[] array)
		{
			this.PackAll<long>(array, delegate (long element)
			{
				this.WriteInt64(element);
			});
		}

		// Token: 0x060004C5 RID: 1221 RVA: 0x0000E887 File Offset: 0x0000CA87
		public void WriteUIntArray(uint[] array)
		{
			this.PackAll<uint>(array, delegate (uint element)
			{
				this.WriteUInt32(element);
			});
		}

		// Token: 0x060004C6 RID: 1222 RVA: 0x0000E89D File Offset: 0x0000CA9D
		public void WriteUInt64Array(ulong[] array)
		{
			this.PackAll<ulong>(array, delegate (ulong element)
			{
				this.WriteUInt64(element);
			});
		}

		// Token: 0x060004C7 RID: 1223 RVA: 0x0000E8B3 File Offset: 0x0000CAB3
		public void WriteShortArray(short[] array)
		{
			this.PackAll<short>(array, delegate (short element)
			{
				this.WriteShort(element);
			});
		}

		// Token: 0x060004C8 RID: 1224 RVA: 0x0000E8C9 File Offset: 0x0000CAC9
		public void WriteUShortArray(ushort[] array)
		{
			this.PackAll<ushort>(array, delegate (ushort element)
			{
				this.WriteUShort(element);
			});
		}

		// Token: 0x060004C9 RID: 1225 RVA: 0x0000E8DF File Offset: 0x0000CADF
		public void WriteFloatArray(float[] array)
		{
			this.PackAll<float>(array, delegate (float element)
			{
				this.WriteFloat(element);
			});
		}

		// Token: 0x060004CA RID: 1226 RVA: 0x0000E8F5 File Offset: 0x0000CAF5
		public void WriteDoubleArray(double[] array)
		{
			this.PackAll<double>(array, delegate (double element)
			{
				this.WriteDouble(element);
			});
		}

		// Token: 0x060004CB RID: 1227 RVA: 0x0000E90C File Offset: 0x0000CB0C
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

		// Token: 0x060004CC RID: 1228 RVA: 0x0000E94C File Offset: 0x0000CB4C
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

		// Token: 0x060004CD RID: 1229 RVA: 0x0000E98C File Offset: 0x0000CB8C
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

		// Token: 0x060004CE RID: 1230 RVA: 0x0000E9CC File Offset: 0x0000CBCC
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

		// Token: 0x060004CF RID: 1231 RVA: 0x0000EA0C File Offset: 0x0000CC0C
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

		// Token: 0x060004D0 RID: 1232 RVA: 0x0000EA4C File Offset: 0x0000CC4C
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

		// Token: 0x060004D1 RID: 1233 RVA: 0x0000EA8C File Offset: 0x0000CC8C
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

		// Token: 0x060004D2 RID: 1234 RVA: 0x0000EACC File Offset: 0x0000CCCC
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

		// Token: 0x060004D3 RID: 1235 RVA: 0x0000EB0C File Offset: 0x0000CD0C
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

		// Token: 0x060004D4 RID: 1236 RVA: 0x0000EB4C File Offset: 0x0000CD4C
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

		// Token: 0x060004F7 RID: 1271 RVA: 0x0000F2E8 File Offset: 0x0000D4E8
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

		// Token: 0x060004F8 RID: 1272 RVA: 0x0000F334 File Offset: 0x0000D534
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

		// Token: 0x060004F9 RID: 1273 RVA: 0x0000F3CC File Offset: 0x0000D5CC
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

		// Token: 0x060004FA RID: 1274 RVA: 0x0000F440 File Offset: 0x0000D640
		public uint ReadBits(int numBits)
		{
			uint result = this.Peek(numBits);
			this.readPos += numBits;
			return result;
		}

		// Token: 0x060004FB RID: 1275 RVA: 0x0000F458 File Offset: 0x0000D658
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

		// Token: 0x060004FC RID: 1276 RVA: 0x0000F4D8 File Offset: 0x0000D6D8
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

		// Token: 0x060004FD RID: 1277 RVA: 0x0000F53C File Offset: 0x0000D73C
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

		// Token: 0x060004FE RID: 1278 RVA: 0x0000F5C0 File Offset: 0x0000D7C0
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

		// Token: 0x060004FF RID: 1279 RVA: 0x0000F644 File Offset: 0x0000D844
		private void ExpandArray()
		{
			uint[] destinationArray = new uint[this.chunks.Length * 2 + 1];
			Array.Copy(this.chunks, destinationArray, this.chunks.Length);
			this.chunks = destinationArray;
		}

		// Token: 0x06000500 RID: 1280 RVA: 0x0000F680 File Offset: 0x0000D880
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

		// Token: 0x06000501 RID: 1281 RVA: 0x0000F6F4 File Offset: 0x0000D8F4
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

		// Token: 0x06000502 RID: 1282 RVA: 0x0000F7B4 File Offset: 0x0000D9B4
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

		// Token: 0x06000503 RID: 1283 RVA: 0x0000F7CB File Offset: 0x0000D9CB
		public void WriteByte(byte val)
		{
			this.WriteBits(8, (uint)val);
		}

		// Token: 0x06000504 RID: 1284 RVA: 0x0000F7D5 File Offset: 0x0000D9D5
		public byte ReadByte()
		{
			return (byte)this.ReadBits(8);
		}

		// Token: 0x06000505 RID: 1285 RVA: 0x0000F7DF File Offset: 0x0000D9DF
		public byte PeekByte()
		{
			return (byte)this.Peek(8);
		}

		// Token: 0x06000506 RID: 1286 RVA: 0x0000F7E9 File Offset: 0x0000D9E9
		public void WriteUInt32(uint value)
		{
			this.WriteBits(32, value);
		}

		// Token: 0x06000507 RID: 1287 RVA: 0x0000F7F4 File Offset: 0x0000D9F4
		public void WriteUInt64(ulong value)
		{
			this.WriteBits(64, value);
		}

		// Token: 0x06000508 RID: 1288 RVA: 0x0000F7FF File Offset: 0x0000D9FF
		public void WriteInt32(int value)
		{
			this.WriteBits(32, (uint)value);
		}

		// Token: 0x06000509 RID: 1289 RVA: 0x0000F80A File Offset: 0x0000DA0A
		public void WriteInt(int value)
		{
			this.WriteInt32(value);
		}

		// Token: 0x0600050A RID: 1290 RVA: 0x0000F813 File Offset: 0x0000DA13
		public void WriteInt64(long value)
		{
			this.WriteBits(64, (ulong)value);
		}

		// Token: 0x0600050B RID: 1291 RVA: 0x0000F81E File Offset: 0x0000DA1E
		public uint ReadUInt32()
		{
			return this.ReadBits(32);
		}

		// Token: 0x0600050C RID: 1292 RVA: 0x0000F828 File Offset: 0x0000DA28
		public ulong ReadUInt64()
		{
			return this.ReadBitsULong(64);
		}

		// Token: 0x0600050D RID: 1293 RVA: 0x0000F832 File Offset: 0x0000DA32
		public int ReadInt32()
		{
			return (int)this.ReadBits(32);
		}

		// Token: 0x0600050E RID: 1294 RVA: 0x0000F83C File Offset: 0x0000DA3C
		public int ReadInt()
		{
			return this.ReadInt32();
		}

		// Token: 0x0600050F RID: 1295 RVA: 0x0000F844 File Offset: 0x0000DA44
		public long ReadInt64()
		{
			return (long)this.ReadBitsULong(64);
		}

		// Token: 0x06000510 RID: 1296 RVA: 0x0000F850 File Offset: 0x0000DA50
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

		// Token: 0x06000511 RID: 1297 RVA: 0x0000F884 File Offset: 0x0000DA84
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

		// Token: 0x06000512 RID: 1298 RVA: 0x0000F8BC File Offset: 0x0000DABC
		public uint PeekVarUInt()
		{
			int num = this.readPos;
			uint result = this.ReadVarUInt();
			this.readPos = num;
			return result;
		}

		// Token: 0x06000513 RID: 1299 RVA: 0x0000F8E0 File Offset: 0x0000DAE0
		public void WriteVarInt(int val)
		{
			uint val2 = (uint)(val << 1 ^ val >> 31);
			this.WriteVarUInt(val2);
		}

		// Token: 0x06000514 RID: 1300 RVA: 0x0000F900 File Offset: 0x0000DB00
		public void WriteVarInt64(long val)
		{
			ulong value = (ulong)(val << 1 ^ val >> 63);
			this.WriteUInt64(value);
		}

		// Token: 0x06000515 RID: 1301 RVA: 0x0000F920 File Offset: 0x0000DB20
		public int ReadVarInt()
		{
			uint num = this.ReadVarUInt();
			return -((int)((ulong)(num >> 1) ^ (ulong)(num & 1U)));

		}

		// Token: 0x06000516 RID: 1302 RVA: 0x0000F940 File Offset: 0x0000DB40
		public long ReadVarInt64()
		{
			ulong num = this.ReadUInt64();
			return (long)((num >> 1 & 9223372036854775807UL) ^ num << 63 >> 63);
		}

		// Token: 0x06000517 RID: 1303 RVA: 0x0000F96C File Offset: 0x0000DB6C
		public int PeekVarInt()
		{
			uint num = this.PeekVarUInt();
			return -((int)((ulong)(num >> 1) ^ (ulong)(num & 1U)));
		}

		// Token: 0x06000518 RID: 1304 RVA: 0x0000F98B File Offset: 0x0000DB8B
		public void WriteBool(bool value)
		{
			this.WriteBits(1, value ? 1U : 0U);
		}

		// Token: 0x06000519 RID: 1305 RVA: 0x0000F99B File Offset: 0x0000DB9B
		public bool ReadBool()
		{
			return this.ReadBits(1) > 0U;
		}

		// Token: 0x0600051A RID: 1306 RVA: 0x0000F9A7 File Offset: 0x0000DBA7
		public bool PeekBool()
		{
			return this.Peek(1) > 0U;
		}

		// Token: 0x0600051B RID: 1307 RVA: 0x0000F9B4 File Offset: 0x0000DBB4
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

		// Token: 0x0600051C RID: 1308 RVA: 0x0000FA10 File Offset: 0x0000DC10
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

		// Token: 0x0600051D RID: 1309 RVA: 0x0000FA58 File Offset: 0x0000DC58
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

		// Token: 0x0400025E RID: 606
		private const int GROW_FACTOR = 2;

		// Token: 0x0400025F RID: 607
		private const int MIN_GROW = 1;

		// Token: 0x04000260 RID: 608
		private const int DEFAULT_CAPACITY = 8;

		// Token: 0x04000261 RID: 609
		internal int readPos;

		// Token: 0x04000262 RID: 610
		internal int writePos;

		// Token: 0x04000263 RID: 611
		private uint[] chunks;

		// Token: 0x04000264 RID: 612
		private static IntCompressor ShortCompressor = new IntCompressor(0, 65535);

		// Token: 0x04000265 RID: 613
		internal static readonly FloatCompressor QuatComponentCompressor = new FloatCompressor(-256f, 255f, 1f);

		// Token: 0x04000266 RID: 614
		internal static readonly IntCompressor QuatIndexCompressor = new IntCompressor(0, 3);

		// Token: 0x04000267 RID: 615
		internal const float FLOAT_PRECISION_MULTI = 255f;

		// Token: 0x04000268 RID: 616
		private const int ASCII_BITS = 7;

		// Token: 0x04000269 RID: 617
		private static readonly int STRING_LENGTH_BITS = FastLog2(63U);

	}
}
