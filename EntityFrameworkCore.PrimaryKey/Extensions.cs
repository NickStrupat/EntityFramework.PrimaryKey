using System;
using System.Reflection;

#if EF_CORE
using Microsoft.EntityFrameworkCore;
namespace EntityFrameworkCore.PrimaryKey {
#else
using System.Data.Entity;
namespace EntityFramework.PrimaryKey {
#endif

	public static class Extensions {
		public static PrimaryKeyDictionary<TEntity> GetPrimaryKey<TEntity>(this DbContext context, TEntity entity)
		where TEntity : class {
			return PrimaryKey.GetFunc<TEntity>(context).Invoke(entity);
		}

		public static PrimaryKeyDictionary<TEntity> GetPrimaryKey<TEntity>(this TEntity entity)
		where TEntity : class {
			return PrimaryKey.GetFunc<TEntity>(typeof(TEntity).GetTypeInfo().Assembly).Invoke(entity);
		}

		public static PrimaryKeyDictionary<TEntity> GetPrimaryKey<TEntity>(this TEntity entity, DbContext context)
		where TEntity : class {
			return PrimaryKey.GetFunc<TEntity>(context).Invoke(entity);
		}

		public static PrimaryKeyDictionary<TEntity> GetPrimaryKey<TDbContext, TEntity>(this TEntity entity)
		where TEntity : class
		where TDbContext : DbContext, new() {
			return PrimaryKey.GetFunc<TDbContext, TEntity>().Invoke(entity);
		}

#if NET40
		internal static Type GetTypeInfo(this Type type) => type;
#endif
	}
}
