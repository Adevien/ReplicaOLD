using System;

namespace Replica
{
	public abstract class SyncObject
	{
		public abstract void ClearChanges();

		[Obsolete("Deprecated: Use ClearChanges instead.")]
		public void Flush()
		{
			this.ClearChanges();
		}

		public abstract void OnSerializeAll(NetworkWriter writer);

		public abstract void OnSerializeDelta(NetworkWriter writer);

		public abstract void OnDeserializeAll(NetworkReader reader);

		public abstract void OnDeserializeDelta(NetworkReader reader);

		public abstract void Reset();

		public Action OnDirty;

		public Func<bool> IsRecording = () => true;
	}
}
