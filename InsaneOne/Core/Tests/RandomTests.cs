using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace InsaneOne.Core.Tests
{
	[TestFixture]
	public class RandomTests
	{
		void PrepareArray(out Vector2[] array)
		{
			array = new[] { Vector2.one, Vector2.up };
		}
		void PrepareList(out List<Vector2> list)
		{
			list = new List<Vector2> {Vector2.one, Vector2.up };
		}

		[Test]
		public void TestRandomArray()
		{
			PrepareArray(out var array);
			var result = array.Random();
			Assert.True(result == Vector2.one || result == Vector2.up);
		}

		[Test]
		public void TestRandomList()
		{
			PrepareList(out var list);
			var result = list.Random();
			Assert.True(result == Vector2.one || result == Vector2.up);
		}

		[Test]
		public void TestRandomArrayMaxLen()
		{
			PrepareArray(out var array);
			var result = array.Random(1);
			Assert.AreEqual(result, Vector2.one);
		}

		[Test]
		public void TestRandomListMaxLen()
		{
			PrepareList(out var list);
			var result = list.Random(1);
			Assert.AreEqual(result, Vector2.one);
		}

		[Test]
		public void TestRandomCondition()
		{
			PrepareArray(out var array);
			var isSuccess = array.TryRandom(v => v.x == 0, out var result);

			Assert.IsTrue(isSuccess);
			Assert.AreEqual(result, Vector2.up);
		}

		[Test]
		public void TestRandomConditionNoSuitable()
		{
			PrepareArray(out var array);
			var isSuccess = array.TryRandom(v => Mathf.Approximately(v.x, 2), out _);

			Assert.AreEqual(isSuccess, false);
		}

		[Test]
		public void TestWeightedRandom()
		{
			// elements with zero are always ignored
			var dict = new Dictionary<int, float>
			{
				{1, 0f},
				{2, 1f},
				{3, 0f},
			};

			var result = dict.GetWeightedItem();

			Assert.AreEqual(result, 2);
		}
	}
}