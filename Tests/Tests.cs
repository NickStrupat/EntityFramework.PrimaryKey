using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

using EntityFramework.PrimaryKey;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests {
	public abstract class TestBase : IDisposable {
		protected readonly Context context = GetContext();

		private static Context GetContext() {
			var context = new Context();
			if (context.Database.Delete())
				context.Database.Create();
			return context;
		}

		public void Dispose() => context.Dispose();
	}

	[TestClass]
	public class SingleColumnByConvention : TestBase {
		A e = new A { Id = 42 };

		[TestMethod] public void GetKeyWithoutContext()      => Asserts(e.GetPrimaryKey());
		[TestMethod] public void GetKeyWithContextInstance() => Asserts(e.GetPrimaryKey(context));
		[TestMethod] public void GetKeyWithContextType()     => Asserts(e.GetPrimaryKey<Context, A>());

		void Asserts(Dictionary<String, Object> key) {
			Assert.AreEqual(1, key.Count);
			Assert.AreEqual(e.Id, key[nameof(A.Id)]);
		}
	}

	[TestClass]
	public class SingleColumnByAttribute : TestBase {
		B e = new B { KeyColumn = 42 };
		
		[TestMethod] public void GetKeyWithoutContext()      => Asserts(e.GetPrimaryKey());
		[TestMethod] public void GetKeyWithContextInstance() => Asserts(e.GetPrimaryKey(context));
		[TestMethod] public void GetKeyWithContextType()     => Asserts(e.GetPrimaryKey<Context, B>());

		void Asserts(Dictionary<String, Object> key) {
			Assert.AreEqual(1, key.Count);
			Assert.AreEqual(e.KeyColumn, key[nameof(B.KeyColumn)]);
		}
	}

	[TestClass]
	public class SingleColumnByFluentMapping : TestBase {
		C e = new C { IdColumn = 42 };
		
		[TestMethod] public void GetKeyWithoutContext()      => Asserts(e.GetPrimaryKey());
		[TestMethod] public void GetKeyWithContextInstance() => Asserts(e.GetPrimaryKey(context));
		[TestMethod] public void GetKeyWithContextType()     => Asserts(e.GetPrimaryKey<Context, C>());

		void Asserts(Dictionary<String, Object> key) {
			Assert.AreEqual(1, key.Count);
			Assert.AreEqual(e.IdColumn, key[nameof(C.IdColumn)]);
		}
	}

	[TestClass]
	public class MultipleColumnByFluentMapping : TestBase {
		D e = new D { Id = 42, Id2 = Guid.NewGuid(), Id3 = Guid.NewGuid(), Id4 = Guid.NewGuid() };

		[TestMethod] public void GetKeyWithoutContext()      => Asserts(e.GetPrimaryKey());
		[TestMethod] public void GetKeyWithContextInstance() => Asserts(e.GetPrimaryKey(context));
		[TestMethod] public void GetKeyWithContextType()     => Asserts(e.GetPrimaryKey<Context, D>());

		void Asserts(Dictionary<String, Object> key) {
			Assert.AreEqual(4, key.Count);
			Assert.AreEqual(e.Id, key[nameof(D.Id)]);
			Assert.AreEqual(e.Id2, key[nameof(D.Id2)]);
			Assert.AreEqual(e.Id3, key[nameof(D.Id3)]);
			Assert.AreEqual(e.Id4, key[nameof(D.Id4)]);
		}
	}

	[TestClass]
	public class MultipleColumnByAttribute : TestBase {
		E e = new E { Id = 42, Id2 = Guid.NewGuid() };

		[TestMethod] public void GetKeyWithoutContext()      => Asserts(e.GetPrimaryKey());
		[TestMethod] public void GetKeyWithContextInstance() => Asserts(e.GetPrimaryKey(context));
		[TestMethod] public void GetKeyWithContextType()     => Asserts(e.GetPrimaryKey<Context, E>());

		void Asserts(Dictionary<String, Object> key) {
			Assert.AreEqual(2, key.Count);
			Assert.AreEqual(e.Id, key[nameof(E.Id)]);
			Assert.AreEqual(e.Id2, key[nameof(E.Id2)]);
		}
	}

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

	public class E {
		[Key, Column(Order = 0)]
		public virtual Int64 Id { get; set; }
		[Key, Column("OtherColumnNameInDb", Order = 1)]
		public virtual Guid Id2 { get; set; }
		public virtual String Name { get; set; }
	}

	public class Context : DbContext {
		public virtual DbSet<A> As { get; protected set; }
		public virtual DbSet<B> Bs { get; protected set; }
		public virtual DbSet<C> Cs { get; protected set; }
		public virtual DbSet<D> Ds { get; protected set; }
		public virtual DbSet<E> Es { get; protected set; }

		protected override void OnModelCreating(DbModelBuilder modelBuilder) {
			modelBuilder.Entity<C>().HasKey(t => t.IdColumn);
			modelBuilder.Entity<D>().HasKey(t => new { t.Id, t.Id2, t.Id3, t.Id4 });
			base.OnModelCreating(modelBuilder);
		}
	}
}
