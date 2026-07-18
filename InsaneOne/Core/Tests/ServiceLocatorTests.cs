using System;
using InsaneOne.Core.Architect;
using NUnit.Framework;

namespace InsaneOne.Core.Tests
{
	[TestFixture]
	public class ServiceLocatorTests
	{
		interface ITestService {}

		class TestServiceA : ITestService {}
		class TestServiceB {}

		[Test]
		public void TestReset()
		{
			try
			{
				ServiceLocator.Reset();
			}
			catch (Exception ex)
			{
				Assert.Fail("Expected no {0} to be thrown", ex.Message);
			}
		}

		void Prepare()
		{
			ServiceLocator.Reset();
		}

		[Test]
		public void TestRegister()
		{
			Prepare();

			try
			{
				ServiceLocator.Register(new TestServiceA());
			}
			catch (Exception ex)
			{
				Assert.Fail("Expected no '{0}' to be thrown", ex.Message);
			}
		}

		[Test]
		public void TestDoubleRegister()
		{
			Prepare();

			ServiceLocator.Register(new TestServiceA());

			var secondRegister = ServiceLocator.Register(new TestServiceA());
			Assert.IsFalse(secondRegister);
		}

		[Test]
		public void TestGet()
		{
			Prepare();
			ServiceLocator.Register(new TestServiceA());

			try
			{
				ServiceLocator.Get<TestServiceA>();
			}
			catch (Exception ex)
			{
				Assert.Fail("Expected no '{0}' to be thrown", ex.Message);
			}
		}

		/// <summary> Test succeed if exception was thrown - we expect this happens when trying to get service without Register. </summary>
		[Test]
		public void TestGetShouldFail()
		{
			Prepare();

			var wasException = false;

			try
			{
				ServiceLocator.Get<TestServiceA>();
			}
			catch (Exception _)
			{
				wasException = true;
			}

			if (!wasException)
				Assert.Fail("Should be exception!");
		}

		[Test]
		public void TestTryGet()
		{
			Prepare();
			ServiceLocator.Register(new TestServiceA());

			var isExist = ServiceLocator.TryGet<TestServiceA>(out var result);

			Assert.True(isExist);
			Assert.NotNull(result);
		}

		[Test]
		public void TestUnregister()
		{
			Prepare();
			ServiceLocator.Register(new TestServiceA());
			ServiceLocator.Unregister<TestServiceA>();

			var isExist = ServiceLocator.TryGet<TestServiceA>(out var result);

			Assert.False(isExist);
			Assert.Null(result);
		}

		[Test]
		public void TestAlias()
		{
			Prepare();
			ServiceLocator.Register(new TestServiceA());

			var isAliased = ServiceLocator.Alias<TestServiceA, ITestService>();

			Assert.True(isAliased);
			Assert.AreSame(ServiceLocator.Get<TestServiceA>(), ServiceLocator.Get<ITestService>());
		}

		[Test]
		public void TestAliasWithoutRegisterShouldFail()
		{
			Prepare();

			var isAliased = ServiceLocator.Alias<TestServiceA, ITestService>();

			Assert.False(isAliased);
		}

		[Test]
		public void TestAliasTakenTwiceShouldFail()
		{
			Prepare();
			ServiceLocator.Register(new TestServiceA());

			ServiceLocator.Alias<TestServiceA, ITestService>();
			var isAliasedTwice = ServiceLocator.Alias<TestServiceA, ITestService>();

			Assert.False(isAliasedTwice);
		}

		[Test]
		public void TestUnregisterRemovesAlias()
		{
			Prepare();
			ServiceLocator.Register(new TestServiceA());
			ServiceLocator.Alias<TestServiceA, ITestService>();

			ServiceLocator.Unregister<TestServiceA>();

			var isExist = ServiceLocator.TryGet<ITestService>(out _);
			Assert.False(isExist);
		}

		[Test]
		public void TestTwoServices()
		{
			Prepare();

			ServiceLocator.Register(new TestServiceA());
			ServiceLocator.Register(new TestServiceB());

			ServiceLocator.Get<TestServiceA>();
			ServiceLocator.Get<TestServiceB>();
		}
	}
}