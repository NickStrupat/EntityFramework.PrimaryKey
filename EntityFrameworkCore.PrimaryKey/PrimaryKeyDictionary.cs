using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

#if NETSTANDARD2_0
namespace EntityFrameworkCore.PrimaryKey {
#else
namespace EntityFramework.PrimaryKey {
#endif
	public class PrimaryKeyDictionary<TEntity> : ReadOnlyDictionary<String, Object>, IEquatable<Dictionary<String, Object>> where TEntity : class {
		public Type EntityType { get; } = typeof(TEntity);

		internal PrimaryKeyDictionary(Dictionary<String, Object> dictionary) : base(dictionary) { }

		public override Boolean Equals(Object other) {
			if (other == null)
				return false;
			if (ReferenceEquals(this, other))
				return true;
			if (!(other is Dictionary<String, Object> pkd))
				return false;
			return EqualsKeysAndValues(pkd);
		}

		public Boolean Equals(Dictionary<String, Object> other) => ReferenceEquals(Dictionary, other) || EqualsKeysAndValues(other);

		private Boolean EqualsKeysAndValues(Dictionary<String, Object> other) {
			if (Keys.Count != other.Keys.Count)
				return false;
			foreach (var key in Keys)
				if (!other.TryGetValue(key, out var otherValue) || !this[key].Equals(otherValue))
					return false;
			return true;
		}

		public override Int32 GetHashCode() {
			var hashCode = 0x51ed270b;
			unchecked {
				foreach (var key in Keys)
					if (key != null)
						hashCode = (hashCode * -1521134295) + key.GetHashCode();
				foreach (var value in Values)
					if (value != null)
						hashCode = (hashCode * -1521134295) + value.GetHashCode();
			}
			return hashCode;
		}

		public class EqualityComparer : IEqualityComparer<PrimaryKeyDictionary<TEntity>> {
			public Boolean Equals(PrimaryKeyDictionary<TEntity> x, PrimaryKeyDictionary<TEntity> y) => x.Equals(y);
			public Int32 GetHashCode(PrimaryKeyDictionary<TEntity> obj) => obj.GetHashCode();
		}
	}
}