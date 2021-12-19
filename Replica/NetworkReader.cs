using System;
using System.IO;

namespace Replica
{
	// Token: 0x0200004C RID: 76
	public class NetworkReader
	{
		// Token: 0x17000032 RID: 50
		// (get) Token: 0x060001EE RID: 494 RVA: 0x00008DC3 File Offset: 0x00006FC3
		public int Length
		{
			get
			{
				return this.buffer.Count;
			}
		}

		// Token: 0x17000033 RID: 51
		// (get) Token: 0x060001EF RID: 495 RVA: 0x00008DD0 File Offset: 0x00006FD0
		public int Remaining
		{
			get
			{
				return this.Length - this.Position;
			}
		}

		// Token: 0x060001F0 RID: 496 RVA: 0x00008DDF File Offset: 0x00006FDF
		public NetworkReader(byte[] bytes)
		{
			this.buffer = new ArraySegment<byte>(bytes);
		}

		// Token: 0x060001F1 RID: 497 RVA: 0x00008DF3 File Offset: 0x00006FF3
		public NetworkReader(ArraySegment<byte> segment)
		{
			this.buffer = segment;
		}

		// Token: 0x060001F2 RID: 498 RVA: 0x00008E02 File Offset: 0x00007002
		public void SetBuffer(byte[] bytes)
		{
			this.buffer = new ArraySegment<byte>(bytes);
			this.Position = 0;
		}

		// Token: 0x060001F3 RID: 499 RVA: 0x00008E17 File Offset: 0x00007017
		public void SetBuffer(ArraySegment<byte> segment)
		{
			this.buffer = segment;
			this.Position = 0;
		}

		// Token: 0x060001F4 RID: 500 RVA: 0x00008E28 File Offset: 0x00007028
		public byte ReadByte()
		{
			if (this.Position + 1 > this.buffer.Count)
			{
				throw new EndOfStreamException("ReadByte out of range:" + this.ToString());
			}
			byte[] array = this.buffer.Array;
			int offset = this.buffer.Offset;
			int position = this.Position;
			this.Position = position + 1;
			return array[offset + position];
		}

		// Token: 0x060001F5 RID: 501 RVA: 0x00008E8C File Offset: 0x0000708C
		public byte[] ReadBytes(byte[] bytes, int count)
		{
			if (count > bytes.Length)
			{
				throw new EndOfStreamException(string.Format("ReadBytes can't read {0} + bytes because the passed byte[] only has length {1}", count, bytes.Length));
			}
			ArraySegment<byte> data = this.ReadBytesSegment(count);
			Array.Copy(data.Array, data.Offset, bytes, 0, count);
			return bytes;
		}

		// Token: 0x060001F6 RID: 502 RVA: 0x00008EDC File Offset: 0x000070DC
		public ArraySegment<byte> ReadBytesSegment(int count)
		{
			if (this.Position + count > this.buffer.Count)
			{
				throw new EndOfStreamException(string.Format("ReadBytesSegment can't read {0} bytes because it would read past the end of the stream. {1}", count, this.ToString()));
			}
			ArraySegment<byte> result = new ArraySegment<byte>(this.buffer.Array, this.buffer.Offset + this.Position, count);
			this.Position += count;
			return result;
		}

		// Token: 0x060001F7 RID: 503 RVA: 0x00008F4C File Offset: 0x0000714C
		public override string ToString()
		{
			return string.Format("NetworkReader pos={0} len={1} buffer={2}", this.Position, this.Length, BitConverter.ToString(this.buffer.Array, this.buffer.Offset, this.buffer.Count));
		}

		// Token: 0x060001F8 RID: 504 RVA: 0x00008FA0 File Offset: 0x000071A0
		public T Read<T>()
		{
			Func<NetworkReader, T> readerDelegate = Reader<T>.read;
			if (readerDelegate == null)
			{
				Console.WriteLine(string.Format("No reader found for {0}. Use a type supported by Mirror or define a custom reader", typeof(T)));
				return default(T);
			}
			return readerDelegate(this);
		}

		// Token: 0x040000EB RID: 235
		private ArraySegment<byte> buffer;

		// Token: 0x040000EC RID: 236
		public int Position;
	}
}
