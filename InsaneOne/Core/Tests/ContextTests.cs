using System.Text.RegularExpressions;
using InsaneOne.Core.Architect;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace InsaneOne.Core.Tests
{
	[TestFixture]
	public class ContextTests
	{
		class TestContext
		{
			public int Value;
		}

		class TestContextReceiver : ContextBehaviour<TestContext> { }

		void Prepare() => Context<TestContext>.Dispose();

		[Test]
		public void TestGet_BeforeInitialize_ReturnsNull()
		{
			Prepare();

			Assert.IsNull(Context<TestContext>.Get());
		}

		[Test]
		public void TestInitialize_SetsGetValue()
		{
			Prepare();
			var context = new TestContext { Value = 7 };

			Context<TestContext>.Initialize(context);

			Assert.AreSame(context, Context<TestContext>.Get());
		}

		[Test]
		public void TestInitialize_PushesContextToExistingReceivers()
		{
			Prepare();
			var go = new GameObject("Test Receiver");
			var receiver = go.AddComponent<TestContextReceiver>();

			Context<TestContext>.Initialize(new TestContext { Value = 5 });

			Assert.AreEqual(5, receiver.Context.Value);
		}

		// regression test: Initialize used to silently no-op on a second call without logging anything,
		// which made an accidentally-missing Dispose() very hard to notice
		[Test]
		public void TestInitialize_CalledTwice_SecondCallIgnoredAndWarns()
		{
			Prepare();
			var first = new TestContext { Value = 1 };
			var second = new TestContext { Value = 2 };

			Context<TestContext>.Initialize(first);

			LogAssert.Expect(LogType.Warning, new Regex("Already initialized"));
			Context<TestContext>.Initialize(second);

			Assert.AreSame(first, Context<TestContext>.Get());
		}

		[Test]
		public void TestDispose_ClearsContextAndAllowsReinitialize()
		{
			Prepare();
			Context<TestContext>.Initialize(new TestContext { Value = 1 });

			Context<TestContext>.Dispose();
			Assert.IsNull(Context<TestContext>.Get());

			var second = new TestContext { Value = 2 };
			Context<TestContext>.Initialize(second);

			Assert.AreSame(second, Context<TestContext>.Get());
		}

		[Test]
		public void TestReset_PushesNewContextToExistingReceivers()
		{
			Prepare();
			var go = new GameObject("Test Receiver");
			var receiver = go.AddComponent<TestContextReceiver>();
			Context<TestContext>.Initialize(new TestContext { Value = 1 });

			var newContext = new TestContext { Value = 9 };
			Context<TestContext>.Reset(newContext);

			Assert.AreEqual(9, receiver.Context.Value);
			Assert.AreSame(newContext, Context<TestContext>.Get());
		}

		// regression test: destroyed receivers used to be checked via a List.Contains() based set;
		// make sure the HashSet-based bookkeeping still drops dead receivers cleanly on Reset
		[Test]
		public void TestReset_RemovesDestroyedReceiversWithoutThrowing()
		{
			Prepare();
			var go = new GameObject("Test Receiver");
			go.AddComponent<TestContextReceiver>();
			Context<TestContext>.Initialize(new TestContext { Value = 1 });

			Object.DestroyImmediate(go);

			Assert.DoesNotThrow(() => Context<TestContext>.Reset(new TestContext { Value = 2 }));
		}

		[Test]
		public void TestAdd_PushesCurrentContextToNewlyAddedReceiver()
		{
			Prepare();
			Context<TestContext>.Initialize(new TestContext { Value = 3 });

			var go = new GameObject("Late Receiver");
			var receiver = go.AddComponent<TestContextReceiver>();
			Context<TestContext>.Add(go);

			Assert.AreEqual(3, receiver.Context.Value);
		}

		[Test]
		public void TestAdd_CalledTwiceForSameObject_DoesNotThrowAndKeepsContext()
		{
			Prepare();
			var go = new GameObject("Test Receiver");
			var receiver = go.AddComponent<TestContextReceiver>();
			Context<TestContext>.Initialize(new TestContext { Value = 1 });

			Assert.DoesNotThrow(() => Context<TestContext>.Add(go));
			Assert.AreEqual(1, receiver.Context.Value);
		}
	}
}
