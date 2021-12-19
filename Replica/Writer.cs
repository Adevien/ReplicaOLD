using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Replica
{
	public static class Writer<T>
	{
		// Token: 0x0400010B RID: 267
		public static Action<NetworkWriter, T> write;
	}
}
