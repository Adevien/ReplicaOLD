using System;
using System.Runtime.InteropServices;

namespace Replica.Runtime
{
	public static class Utils
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

	[StructLayout(LayoutKind.Explicit)]
	internal struct UIntFloat
	{
		[FieldOffset(0)]
		public float FloatValue;

		[FieldOffset(0)]
		public uint UIntValue;

		[FieldOffset(0)]
		public double DoubleValue;

		[FieldOffset(0)]
		public ulong ULongValue;
	}

	public class FloatCompressor
	{
		internal int RequiredBits
		{
			get
			{
				return this.requiredBits;
			}
		}

		public FloatCompressor(float minValue, float maxValue, float precision)
		{
			this.minValue = minValue;
			this.maxValue = maxValue;
			this.precision = precision;
			this.invPrecision = 1f / precision;
			this.requiredBits = this.ComputeRequiredBits();
			this.mask = (uint)((1L << this.requiredBits) - 1L);
		}

		public uint Pack(float value)
		{
			Utils.Clamp(value, this.minValue, this.maxValue);
			return (uint)((value - this.minValue) * this.invPrecision + 0.5f) & this.mask;
		}

		public float Unpack(uint data)
		{
			return Utils.Clamp(data * this.precision + this.minValue, this.minValue, this.maxValue);
		}

		private int ComputeRequiredBits()
		{
			return Utils.FastLog2((uint)((this.maxValue - this.minValue) * (1f / this.precision) + 0.5f)) + 1;
		}

		private readonly float precision;

		private readonly float invPrecision;

		private readonly float minValue;

		private readonly float maxValue;

		private readonly int requiredBits;

		private readonly uint mask;
	}

	public class IntCompressor
	{
		internal int RequiredBits
		{
			get
			{
				return this.requiredBits;
			}
		}

		public IntCompressor(int minValue, int maxValue)
		{
			this.minValue = minValue;
			this.maxValue = maxValue;
			this.requiredBits = this.ComputeRequiredBits();
			this.mask = (uint)((1L << this.requiredBits) - 1L);
		}

		public uint Pack(int value)
		{
			if (value >= this.minValue)
			{
				int num = this.maxValue;
			}
			return (uint)(value - this.minValue & (int)this.mask);
		}

		public int Unpack(uint data)
		{
			return (int)((ulong)data + (ulong)((long)this.minValue));
		}

		private int ComputeRequiredBits()
		{
			if (this.minValue >= this.maxValue)
			{
				return 0;
			}
			long num = (long)this.minValue;
			return Utils.FastLog2((uint)((long)this.maxValue - num)) + 1;
		}

		private readonly int minValue;

		private readonly int maxValue;

		private readonly int requiredBits;

		private readonly uint mask;
	}

	public static class IntCompressorExtensions
	{
		public static void WriteInt(this NetBuffer buffer, IntCompressor compressor, int value)
		{
			if (compressor.RequiredBits > 311)
			{
				buffer.WriteVarUInt(compressor.Pack(value));
				return;
			}
			buffer.WriteBits(compressor.RequiredBits, compressor.Pack(value));
		}

		public static int ReadInt(this NetBuffer buffer, IntCompressor compressor)
		{
			if (compressor.RequiredBits > 311)
			{
				return compressor.Unpack(buffer.ReadVarUInt());
			}
			return compressor.Unpack(buffer.ReadBits(compressor.RequiredBits));
		}

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