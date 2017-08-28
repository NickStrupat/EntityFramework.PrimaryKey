using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

#if NETSTANDARD2_0
using Microsoft.EntityFrameworkCore;
namespace EntityFrameworkCore.PrimaryKey {
#else
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
namespace EntityFramework.PrimaryKey {
#endif
	public static class PrimaryKey {
		public static Func<TEntity, PrimaryKeyDictionary<TEntity>> GetFunc<TDbContext, TEntity>() where TEntity : class where TDbContext : DbContext, new() {
			return PerDbContextTypeCache<TEntity>.Map.GetOrAdd(typeof(TDbContext),
				type => {
					using (var context = new TDbContext())
						return GetFunc<TEntity>(context);
				});
		}

		public static Func<TEntity, PrimaryKeyDictionary<TEntity>> GetFunc<TEntity>(DbContext context) where TEntity : class {
			return PerDbContextTypeCache<TEntity>.Map.GetOrAdd(context.GetType(),
				type => {
					PropertyInfo[] keyProperties;
					using (var con = (DbContext) Activator.CreateInstance(type)) { // Make a new instance inside the lambda so we don't capture the parameter
#if NETSTANDARD2_0
						var keyNames = con.Model.FindEntityType(typeof(TEntity)).FindPrimaryKey().Properties.Select(x => x.Name);
#else
						IObjectContextAdapter oca = con;
						var keyNames = oca.ObjectContext.CreateObjectSet<TEntity>().EntitySet.ElementType.KeyMembers.Select(x => x.Name);
#endif
						keyProperties = typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => keyNames.Contains(x.Name)).ToArray();
					}
					return entity => {
						var dictionary = new Dictionary<String, Object>(keyProperties.Length);
						foreach (var keyProperty in keyProperties)
							dictionary.Add(keyProperty.Name, GetPropertyGetterFunc<TEntity>(keyProperty).Invoke(entity));
						return new PrimaryKeyDictionary<TEntity>(dictionary);
					};
				});
		}

		private static Func<TEntity, Object> GetPropertyGetterFunc<TEntity>(PropertyInfo propertyInfo) {
			var instance = Expression.Parameter(typeof(TEntity));
			var call = Expression.Call(instance, propertyInfo.GetGetMethod());
			return Expression.Lambda<Func<TEntity, Object>>(Expression.Convert(call, typeof(Object)), instance).Compile();
		}

		private static class PerDbContextTypeCache<TEntity> where TEntity : class {
			public static readonly ConcurrentDictionary<Type, Func<TEntity, PrimaryKeyDictionary<TEntity>>> Map = new ConcurrentDictionary<Type, Func<TEntity, PrimaryKeyDictionary<TEntity>>>();
		}

		public static Func<TEntity, PrimaryKeyDictionary<TEntity>> GetFunc<TEntity>() where TEntity : class {
			return GetFunc<TEntity>(typeof(TEntity).GetTypeInfo().Assembly);
		}

		internal static Func<TEntity, PrimaryKeyDictionary<TEntity>> GetFunc<TEntity>(Assembly assembly) where TEntity : class {
			return PerAssemblyCache<TEntity>.Map.GetOrAdd(assembly,
				a => {
					var contextType = a.GetTypes().SingleOrDefault(x => typeof(DbContext).IsAssignableFrom(x) && x.GetConstructor(Type.EmptyTypes) != null);
					if (contextType == null)
						throw new Exception("Detecting primary keys without supplying a DbContext class only works when called from an assembly which contains exactly one DbContext-derived class with a public parameterless constructor");
					using (var context = (DbContext) Activator.CreateInstance(contextType))
						return GetFunc<TEntity>(context);
				});
		}

		private static class PerAssemblyCache<TEntity> where TEntity : class {
			public static readonly ConcurrentDictionary<Assembly, Func<TEntity, PrimaryKeyDictionary<TEntity>>> Map = new ConcurrentDictionary<Assembly, Func<TEntity, PrimaryKeyDictionary<TEntity>>>();
		}
	}
}