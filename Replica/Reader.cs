using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Replica
{
	public static class Reader<T>
	{
		// Token: 0x040000EA RID: 234
		public static Func<NetworkReader, T> read;
	}
}
