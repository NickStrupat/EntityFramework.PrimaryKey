using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace EntityFramework.PrimaryKey {
	public class PrimaryKeyDictionary<TEntity> : ReadOnlyDictionary<String, Object>, IEquatable<Dictionary<String, Object>> where TEntity : class {
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
			if (ReferenceEquals(this.Dictionary, other))
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
			foreach (var key in Keys)
				if (key != null)
					hashCode = (hashCode * -1521134295) + key.GetHashCode();
			foreach (var value in Values)
				if (value != null)
					hashCode = (hashCode * -1521134295) + value.GetHashCode();
			return hashCode;
		}
	}
}