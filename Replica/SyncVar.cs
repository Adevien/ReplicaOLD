using System;
using System.Collections.Generic;

namespace Replica
{
	// Token: 0x02000072 RID: 114
	[Serializable]
	public class SyncVar<T> : SyncObject, IEquatable<T>
	{
		public virtual T Value
		{
			get
			{
				return this._Value;
			}
			set
			{
				if (!this.Equals(value))
				{
					T old = this._Value;
					this._Value = value;
					this.OnDirty();
					if (this.hook != null && !this.hookGuard /*&& NetworkClient.active*/)
					{
						this.hookGuard = true;
						this.hook(old, value);
						this.hookGuard = false;
					}
				}
			}
		}

		public override void ClearChanges()
		{
		}

		public override void Reset()
		{
		}

		public SyncVar(T value, Action<T, T> hook = null)
		{
			this._Value = value;
			this.hook = hook;
		}

		public static implicit operator T(SyncVar<T> field)
		{
			return field.Value;
		}

		public static implicit operator SyncVar<T>(T value)
		{
			return new SyncVar<T>(value, null);
		}

		public override void OnSerializeAll(NetworkWriter writer)
		{
			writer.Write<T>(this.Value);
		}

		public override void OnSerializeDelta(NetworkWriter writer)
		{
			writer.Write<T>(this.Value);
		}

		public override void OnDeserializeAll(NetworkReader reader)
		{
			this.Value = reader.Read<T>();
		}

		public override void OnDeserializeDelta(NetworkReader reader)
		{
			this.Value = reader.Read<T>();
		}

		public bool Equals(T other)
		{
			return EqualityComparer<T>.Default.Equals(this.Value, other);
		}

		public override string ToString()
		{
			T value = this.Value;
			return value.ToString();
		}

		private T _Value;

		private readonly Action<T, T> hook;

		private bool hookGuard;
	}
}
