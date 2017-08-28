using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Xunit;

#if NETSTANDARD2_0
using Microsoft.EntityFrameworkCore;
namespace EntityFrameworkCore.PrimaryKey.TestsCore {
#else
using System.Data.Entity;
using EntityFramework.PrimaryKey;
namespace EntityFramework.PrimaryKey.Tests {
#endif

	public abstract class TestBase : IDisposable {
		protected readonly Context context = new Context();
		public void Dispose() => context.Dispose();
	}
	
	public class SingleColumnByConvention : TestBase {
		A e = new A { Id = 42L };

		[Fact] public void GetKeyWithoutContext()      => Asserts(e.GetPrimaryKey());
		[Fact] public void GetKeyWithContextInstance() => Asserts(e.GetPrimaryKey(context));
		[Fact] public void GetKeyWithContextType()     => Asserts(e.GetPrimaryKey<Context, A>());

		void Asserts(IDictionary<String, Object> key) {
			Assert.Equal(1, key.Count);
			Assert.Equal(e.Id, key[nameof(A.Id)]);
			Assert.True(key.Equals(new Dictionary<String, Object> { [nameof(A.Id)] = e.Id }));
		}
	}
	
	public class SingleColumnByAttribute : TestBase {
		B e = new B { KeyColumn = 42L };
		
		[Fact] public void GetKeyWithoutContext()      => Asserts(e.GetPrimaryKey());
		[Fact] public void GetKeyWithContextInstance() => Asserts(e.GetPrimaryKey(context));
		[Fact] public void GetKeyWithContextType()     => Asserts(e.GetPrimaryKey<Context, B>());

		void Asserts(IDictionary<String, Object> key) {
			Assert.Equal(1, key.Count);
			Assert.Equal(e.KeyColumn, key[nameof(B.KeyColumn)]);
			Assert.True(key.Equals(new Dictionary <String, Object> { [nameof(B.KeyColumn)] = e.KeyColumn }));
		}
	}

	public class SingleColumnByFluentMapping : TestBase {
		C e = new C { IdColumn = 42L };
		
		[Fact] public void GetKeyWithoutContext()      => Asserts(e.GetPrimaryKey());
		[Fact] public void GetKeyWithContextInstance() => Asserts(e.GetPrimaryKey(context));
		[Fact] public void GetKeyWithContextType()     => Asserts(e.GetPrimaryKey<Context, C>());

		void Asserts(IDictionary<String, Object> key) {
			Assert.Equal(1, key.Count);
			Assert.Equal(e.IdColumn, key[nameof(C.IdColumn)]);
			Assert.True(key.Equals(new Dictionary<String, Object> { [nameof(C.IdColumn)] = e.IdColumn }));
		}
	}

	public class MultipleColumnByFluentMapping : TestBase {
		D e = new D { Id = 42L, Id2 = Guid.NewGuid(), Id3 = Guid.NewGuid(), Id4 = Guid.NewGuid() };

		[Fact] public void GetKeyWithoutContext()      => Asserts(e.GetPrimaryKey());
		[Fact] public void GetKeyWithContextInstance() => Asserts(e.GetPrimaryKey(context));
		[Fact] public void GetKeyWithContextType()     => Asserts(e.GetPrimaryKey<Context, D>());

		void Asserts(IDictionary<String, Object> key) {
			Assert.Equal(4, key.Count);
			Assert.Equal(e.Id, key[nameof(D.Id)]);
			Assert.Equal(e.Id2, key[nameof(D.Id2)]);
			Assert.Equal(e.Id3, key[nameof(D.Id3)]);
			Assert.Equal(e.Id4, key[nameof(D.Id4)]);
			Assert.True(key.Equals(new Dictionary<String, Object> { [nameof(D.Id)] = e.Id, [nameof(D.Id2)] = e.Id2, [nameof(D.Id3)] = e.Id3, [nameof(D.Id4)] = e.Id4 }));
		}
	}

#if !NETSTANDARD2_0
	public class MultipleColumnByAttribute : TestBase {
		E e = new E { Id = 42L, Id2 = Guid.NewGuid() };

		[Fact] public void GetKeyWithoutContext()      => Asserts(e.GetPrimaryKey());
		[Fact] public void GetKeyWithContextInstance() => Asserts(e.GetPrimaryKey(context));
		[Fact] public void GetKeyWithContextType()     => Asserts(e.GetPrimaryKey<Context, E>());

		void Asserts(IDictionary<String, Object> key) {
			Assert.Equal(2, key.Count);
			Assert.Equal(e.Id, key[nameof(E.Id)]);
			Assert.Equal(e.Id2, key[nameof(E.Id2)]);
			Assert.True(key.Equals(new Dictionary<String, Object> { [nameof(E.Id)] = e.Id, [nameof(E.Id2)] = e.Id2 }));
		}
	}
#endif

	public class A {
		public virtual Int64 Id { get; set; }
		public virtual String Name { get; set; }
	}

	public class B {
		[Key]
		public virtual Int64 KeyColumn { get; set; }
		public virtual String Name { get; set; }
	}

	public class C {
		public virtual Int64 IdColumn { get; set; }
		public virtual String Name { get; set; }
	}

	public class D {
		public virtual Int64 Id { get; set; }
		public virtual Guid Id2 { get; set; }
		public virtual Guid Id3 { get; set; }
		public virtual Guid Id4 { get; set; }
		public virtual String Name { get; set; }
	}

#if !NETSTANDARD2_0
	public class E {
		[Key, Column(Order = 0)]
		public virtual Int64 Id { get; set; }
		[Key, Column("OtherColumnNameInDb", Order = 1)]
		public virtual Guid Id2 { get; set; }
		public virtual String Name { get; set; }
	}
#endif

	public class Context : DbContext {
		public virtual DbSet<A> As { get; protected set; }
		public virtual DbSet<B> Bs { get; protected set; }
		public virtual DbSet<C> Cs { get; protected set; }
		public virtual DbSet<D> Ds { get; protected set; }
#if !NETSTANDARD2_0
		public virtual DbSet<E> Es { get; protected set; }
#endif

#if NETSTANDARD2_0
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
			base.OnConfiguring(optionsBuilder);
			optionsBuilder.UseInMemoryDatabase("Tests");
		}
		protected override void OnModelCreating(ModelBuilder modelBuilder) {
#else
		protected override void OnModelCreating(DbModelBuilder modelBuilder) {
#endif
			modelBuilder.Entity<C>().HasKey(t => t.IdColumn);
			modelBuilder.Entity<D>().HasKey(t => new { t.Id, t.Id2, t.Id3, t.Id4 });
			base.OnModelCreating(modelBuilder);
		}
	}
}
