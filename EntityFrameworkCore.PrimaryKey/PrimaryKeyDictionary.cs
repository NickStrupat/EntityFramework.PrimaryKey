using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

#if EF_CORE
namespace EntityFrameworkCore.PrimaryKey {
#else
namespace EntityFramework.PrimaryKey {
#endif
#if NET40
	public class PrimaryKeyDictionary<TEntity> : Dictionary<String, Object>, IEquatable<Dictionary<String, Object>> where TEntity : class {
#else
	public class PrimaryKeyDictionary<TEntity> : ReadOnlyDictionary<String, Object>, IEquatable<Dictionary<String, Object>> where TEntity : class {
#endif
		public Type EntityType { get; } = typeof(TEntity);

		internal PrimaryKeyDictionary(Dictionary<String, Object> dictionary) : base(dictionary) { }

		public override Boolean Equals(Object other) {
			if (other == null)
				return false;
			if (ReferenceEquals(this, other))
				return true;
			var pkd = other as Dictionary<String, Object>;
			if (pkd == null)
				return false;
			return EqualsKeysAndValues(pkd);
		}

		public Boolean Equals(Dictionary<String, Object> other) {
#if NET40
			if (ReferenceEquals(this, other))
#else
			if (ReferenceEquals(this.Dictionary, other))
#endif
				return true;
			return EqualsKeysAndValues(other);
		}

		private Boolean EqualsKeysAndValues(Dictionary<String, Object> other) {
			if (Keys.Count != other.Keys.Count)
				return false;
			foreach (var key in Keys) {
				Object otherValue;
				if (!other.TryGetValue(key, out otherValue))
					return false;
				if (!this[key].Equals(otherValue))
					return false;
			}
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