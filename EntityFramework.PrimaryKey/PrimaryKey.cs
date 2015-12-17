using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace EntityFramework.PrimaryKey {
	public static class PrimaryKey {
		public static Func<TEntity, Dictionary<String, Object>> GetFunc<TDbContext, TEntity>() where TEntity : class where TDbContext : DbContext, new() {
			return PerDbContextCache<TEntity>.Map.GetOrAdd(typeof(TDbContext),
				type => {
					using (var context = new TDbContext())
						return GetFunc<TEntity>(context);
				});
		}

		public static Func<TEntity, Dictionary<String, Object>> GetFunc<TEntity>(DbContext context) where TEntity : class {
			return PerDbContextCache<TEntity>.Map.GetOrAdd(context.GetType(),
				type => {
					IObjectContextAdapter oca = context;
					var keyNames = oca.ObjectContext.CreateObjectSet<TEntity>().EntitySet.ElementType.KeyMembers.Select(x => x.Name);
					var keyProperties = typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => keyNames.Contains(x.Name));
					return entity => keyProperties.ToDictionary(x => x.Name, x => GetPropertyGetterFunc<TEntity>(x).Invoke(entity));
				});
		}

		private static Func<TEntity, Object> GetPropertyGetterFunc<TEntity>(PropertyInfo propertyInfo) {
			var instance = Expression.Parameter(typeof(TEntity));
			var call = Expression.Call(instance, propertyInfo.GetGetMethod());
			return Expression.Lambda<Func<TEntity, Object>>(Expression.Convert(call, typeof(Object)), instance).Compile();
		}

		private static class PerDbContextCache<TEntity> {
			public static readonly ConcurrentDictionary<Type, Func<TEntity, Dictionary<String, Object>>> Map = new ConcurrentDictionary<Type, Func<TEntity, Dictionary<String, Object>>>();
		}

		public static Func<TEntity, Dictionary<String, Object>> GetFunc<TEntity>() where TEntity : class {
			return PerAssemblyCache<TEntity>.Map.GetOrAdd(Assembly.GetCallingAssembly(),
				assembly => {
					var contextType = assembly.GetTypes().SingleOrDefault(x => typeof(DbContext).IsAssignableFrom(x) && x.GetConstructor(Type.EmptyTypes) != null);
					if (contextType == null)
						throw new Exception("Detecting primary keys without supplying a DbContext class only works when called from an assembly which contains exactly one DbContext-derived class with a public parameterless constructor");
					using (var context = (DbContext) Activator.CreateInstance(contextType))
						return GetFunc<TEntity>(context);
				});
		}

		internal static Func<TEntity, Dictionary<String, Object>> GetFunc<TEntity>(Assembly assembly) where TEntity : class {
			return PerAssemblyCache<TEntity>.Map.GetOrAdd(assembly,
				a => {
					var contextType = a.GetTypes().SingleOrDefault(x => typeof(DbContext).IsAssignableFrom(x) && x.GetConstructor(Type.EmptyTypes) != null);
					if (contextType == null)
						throw new Exception("Detecting primary keys without supplying a DbContext class only works when called from an assembly which contains exactly one DbContext-derived class with a public parameterless constructor");
					using (var context = (DbContext) Activator.CreateInstance(contextType))
						return GetFunc<TEntity>(context);
				});
		}

		private static class PerAssemblyCache<TEntity> {
			public static readonly ConcurrentDictionary<Assembly, Func<TEntity, Dictionary<String, Object>>> Map = new ConcurrentDictionary<Assembly, Func<TEntity, Dictionary<String, Object>>>();
		}
	}
}