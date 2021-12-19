using System;
using System.Runtime.CompilerServices;

namespace Replica
{
	// Token: 0x02000058 RID: 88
	public class NetworkWriter
	{
		// Token: 0x060002A7 RID: 679 RVA: 0x0000B4DF File Offset: 0x000096DF
		public void Reset()
		{
			this.Position = 0;
		}

		// Token: 0x060002A8 RID: 680 RVA: 0x0000B4E8 File Offset: 0x000096E8
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void EnsureCapacity(int value)
		{
			if (this.buffer.Length < value)
			{
				int capacity = Math.Max(value, this.buffer.Length * 2);
				Array.Resize<byte>(ref this.buffer, capacity);
			}
		}

		// Token: 0x060002A9 RID: 681 RVA: 0x0000B520 File Offset: 0x00009720
		public byte[] ToArray()
		{
			byte[] data = new byte[this.Position];
			Array.ConstrainedCopy(this.buffer, 0, data, 0, this.Position);
			return data;
		}

		// Token: 0x060002AA RID: 682 RVA: 0x0000B54E File Offset: 0x0000974E
		public ArraySegment<byte> ToArraySegment()
		{
			return new ArraySegment<byte>(this.buffer, 0, this.Position);
		}

		// Token: 0x060002AB RID: 683 RVA: 0x0000B564 File Offset: 0x00009764
		public void WriteByte(byte value)
		{
			this.EnsureCapacity(this.Position + 1);
			byte[] array = this.buffer;
			int position = this.Position;
			this.Position = position + 1;
			array[position] = value;
		}

		// Token: 0x060002AC RID: 684 RVA: 0x0000B598 File Offset: 0x00009798
		public void WriteBytes(byte[] buffer, int offset, int count)
		{
			this.EnsureCapacity(this.Position + count);
			Array.ConstrainedCopy(buffer, offset, this.buffer, this.Position, count);
			this.Position += count;
		}

		// Token: 0x060002AD RID: 685 RVA: 0x0000B5CC File Offset: 0x000097CC
		public void Write<T>(T value)
		{
			Action<NetworkWriter, T> writeDelegate = Writer<T>.write;
			if (writeDelegate == null)
			{
				Console.Write(string.Format("No writer found for {0}. This happens either if you are missing a NetworkWriter extension for your custom type, or if weaving failed. Try to reimport a script to weave again.", typeof(T)));
				return;
			}
			writeDelegate(this, value);
		}

		// Token: 0x0400010C RID: 268
		public const int MaxStringLength = 32768;

		// Token: 0x0400010D RID: 269
		private byte[] buffer = new byte[1500];

		// Token: 0x0400010E RID: 270
		public int Position;
	}
}
