using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Reflection;

namespace EntityFramework.PrimaryKey
{
	public static class Extensions {
		public static Dictionary<String, Object> GetPrimaryKey<TEntity>(this DbContext context, TEntity entity) where TEntity : class => PrimaryKey.GetFunc<TEntity>(context).Invoke(entity);
		public static Dictionary<String, Object> GetPrimaryKey<TEntity>(this TEntity entity) where TEntity : class => PrimaryKey.GetFunc<TEntity>(Assembly.GetCallingAssembly()).Invoke(entity);
		public static Dictionary<String, Object> GetPrimaryKey<TEntity>(this TEntity entity, DbContext context) where TEntity : class => PrimaryKey.GetFunc<TEntity>(context).Invoke(entity);
		public static Dictionary<String, Object> GetPrimaryKey<TDbContext, TEntity>(this TEntity entity) where TEntity : class where TDbContext : DbContext, new() => PrimaryKey.GetFunc<TDbContext, TEntity>().Invoke(entity);
	}
}
