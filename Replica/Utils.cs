using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static Replica.Utils;

namespace Replica
{
    public class Utils
    {
		public static int FastLog2(uint v)
		{
			v |= v >> 1;
			v |= v >> 2;
			v |= v >> 4;
			v |= v >> 8;
			v |= v >> 16;
			return DeBruijnLookup[(int)(v * 130329821U >> 27)];
		}

		public static int FastLog2(ulong value)
		{
			value |= value >> 1;
			value |= value >> 2;
			value |= value >> 4;
			value |= value >> 8;
			value |= value >> 16;
			value |= value >> 32;
			return LookupTable[(int)(checked((IntPtr)(unchecked((value - (value >> 1)) * 571347909858961602UL) >> 58)))];
		}

		public static readonly int[] DeBruijnLookup = new int[]
		{
			0,
			9,
			1,
			10,
			13,
			21,
			2,
			29,
			11,
			14,
			16,
			18,
			22,
			25,
			3,
			30,
			8,
			12,
			20,
			28,
			15,
			17,
			24,
			7,
			19,
			27,
			23,
			6,
			26,
			5,
			4,
			31
		};

		// Token: 0x0400025D RID: 605
		public static readonly int[] LookupTable = new int[]
		{
			63,
			0,
			58,
			1,
			59,
			47,
			53,
			2,
			60,
			39,
			48,
			27,
			54,
			33,
			42,
			3,
			61,
			51,
			37,
			40,
			49,
			18,
			28,
			20,
			55,
			30,
			34,
			11,
			43,
			14,
			22,
			4,
			62,
			57,
			46,
			52,
			38,
			26,
			32,
			41,
			50,
			36,
			17,
			19,
			29,
			10,
			13,
			21,
			56,
			45,
			25,
			31,
			35,
			16,
			9,
			12,
			44,
			24,
			15,
			8,
			23,
			7,
			6,
			5
		};

		[StructLayout(LayoutKind.Explicit)]
		internal struct UIntFloat
		{
			// Token: 0x04000258 RID: 600
			[FieldOffset(0)]
			public float FloatValue;

			// Token: 0x04000259 RID: 601
			[FieldOffset(0)]
			public uint UIntValue;

			// Token: 0x0400025A RID: 602
			[FieldOffset(0)]
			public double DoubleValue;

			// Token: 0x0400025B RID: 603
			[FieldOffset(0)]
			public ulong ULongValue;
		}

		public class FloatCompressor
		{
			// Token: 0x170000DB RID: 219
			// (get) Token: 0x0600047F RID: 1151 RVA: 0x0000E053 File Offset: 0x0000C253
			internal int RequiredBits
			{
				get
				{
					return this.requiredBits;
				}
			}

			// Token: 0x06000480 RID: 1152 RVA: 0x0000E05C File Offset: 0x0000C25C
			public FloatCompressor(float minValue, float maxValue, float precision)
			{
				this.minValue = minValue;
				this.maxValue = maxValue;
				this.precision = precision;
				this.invPrecision = 1f / precision;
				this.requiredBits = this.ComputeRequiredBits();
				this.mask = (uint)((1L << this.requiredBits) - 1L);
			}

			// Token: 0x06000481 RID: 1153 RVA: 0x0000E0B3 File Offset: 0x0000C2B3
			public uint Pack(float value)
			{
				Clamp(value, this.minValue, this.maxValue);
				return (uint)((value - this.minValue) * this.invPrecision + 0.5f) & this.mask;
			}

			// Token: 0x06000482 RID: 1154 RVA: 0x0000E0E7 File Offset: 0x0000C2E7
			public float Unpack(uint data)
			{
				return Clamp(data * this.precision + this.minValue, this.minValue, this.maxValue);
			}

			// Token: 0x06000483 RID: 1155 RVA: 0x0000E10B File Offset: 0x0000C30B
			private int ComputeRequiredBits()
			{
				return FastLog2((uint)((this.maxValue - this.minValue) * (1f / this.precision) + 0.5f)) + 1;
			}

			// Token: 0x04000238 RID: 568
			private readonly float precision;

			// Token: 0x04000239 RID: 569
			private readonly float invPrecision;

			// Token: 0x0400023A RID: 570
			private readonly float minValue;

			// Token: 0x0400023B RID: 571
			private readonly float maxValue;

			// Token: 0x0400023C RID: 572
			private readonly int requiredBits;

			// Token: 0x0400023D RID: 573
			private readonly uint mask;
		}

		public class IntCompressor
		{
			// Token: 0x170000DC RID: 220
			// (get) Token: 0x06000487 RID: 1159 RVA: 0x0000E1C1 File Offset: 0x0000C3C1
			internal int RequiredBits
			{
				get
				{
					return this.requiredBits;
				}
			}

			// Token: 0x06000488 RID: 1160 RVA: 0x0000E1C9 File Offset: 0x0000C3C9
			public IntCompressor(int minValue, int maxValue)
			{
				this.minValue = minValue;
				this.maxValue = maxValue;
				this.requiredBits = this.ComputeRequiredBits();
				this.mask = (uint)((1L << this.requiredBits) - 1L);
			}

			// Token: 0x06000489 RID: 1161 RVA: 0x0000E201 File Offset: 0x0000C401
			public uint Pack(int value)
			{
				if (value >= this.minValue)
				{
					int num = this.maxValue;
				}
				return (uint)(value - this.minValue & (int)this.mask);
			}

			// Token: 0x0600048A RID: 1162 RVA: 0x0000E224 File Offset: 0x0000C424
			public int Unpack(uint data)
			{
				return (int)((ulong)data + (ulong)((long)this.minValue));
			}

			// Token: 0x0600048B RID: 1163 RVA: 0x0000E234 File Offset: 0x0000C434
			private int ComputeRequiredBits()
			{
				if (this.minValue >= this.maxValue)
				{
					return 0;
				}
				long num = (long)this.minValue;
				return FastLog2((uint)((long)this.maxValue - num)) + 1;
			}

			// Token: 0x0400023E RID: 574
			private readonly int minValue;

			// Token: 0x0400023F RID: 575
			private readonly int maxValue;

			// Token: 0x04000240 RID: 576
			private readonly int requiredBits;

			// Token: 0x04000241 RID: 577
			private readonly uint mask;
		}

		public static float Clamp(float value, float min, float max)
		{
			if (value < min)
			{
				value = min;
			}
			else if (value > max)
			{
				value = max;
			}
			return value;
		}
	}

	public static class IntCompressorExtensions
	{
		// Token: 0x06000484 RID: 1156 RVA: 0x0000E135 File Offset: 0x0000C335
		public static void WriteInt(this NetBuffer buffer, IntCompressor compressor, int value)
		{
			if (compressor.RequiredBits > 311)
			{
				buffer.WriteVarUInt(compressor.Pack(value));
				return;
			}
			buffer.WriteBits(compressor.RequiredBits, compressor.Pack(value));
		}

		// Token: 0x06000485 RID: 1157 RVA: 0x0000E165 File Offset: 0x0000C365
		public static int ReadInt(this NetBuffer buffer, IntCompressor compressor)
		{
			if (compressor.RequiredBits > 311)
			{
				return compressor.Unpack(buffer.ReadVarUInt());
			}
			return compressor.Unpack(buffer.ReadBits(compressor.RequiredBits));
		}

		// Token: 0x06000486 RID: 1158 RVA: 0x0000E193 File Offset: 0x0000C393
		public static int PeekInt(this NetBuffer buffer, IntCompressor compressor)
		{
			if (compressor.RequiredBits > 311)
			{
				return compressor.Unpack(buffer.PeekVarUInt());
			}
			return compressor.Unpack(buffer.Peek(compressor.RequiredBits));
		}
	}
}