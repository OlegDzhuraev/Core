using InsaneOne.Core.Utility;
using NUnit.Framework;
using UnityEngine;

namespace InsaneOne.Core.Tests
{
	[TestFixture]
	public class Angle2dUtilityTests
	{
		[Test]
		public void TestGetAngleByPositionOnCircle_CountZero_ReturnsZero()
		{
			var angle = Angle2dUtility.GetAngleByPositionOnCircle(10f, 0, 0);

			Assert.AreEqual(0f, angle);
		}

		[Test]
		public void TestGetAngleByPositionOnCircle_CountOne_ReturnsZero()
		{
			var angle = Angle2dUtility.GetAngleByPositionOnCircle(10f, 0, 1);

			Assert.AreEqual(0f, angle);
		}

		// regression test: halfAngle used to be computed from count instead of (count - 1),
		// which inflated the spacing between neighbours above angleOffset (2x for count=2)
		[Test]
		public void TestGetAngleByPositionOnCircle_TwoPositions_SpacingEqualsOffset()
		{
			var first = Angle2dUtility.GetAngleByPositionOnCircle(10f, 0, 2);
			var second = Angle2dUtility.GetAngleByPositionOnCircle(10f, 1, 2);

			Assert.AreEqual(-5f, first, 0.001f);
			Assert.AreEqual(5f, second, 0.001f);
		}

		[Test]
		public void TestGetAngleByPositionOnCircle_OddCount_CentersOnZeroWithCorrectSpacing()
		{
			var first = Angle2dUtility.GetAngleByPositionOnCircle(10f, 0, 3);
			var middle = Angle2dUtility.GetAngleByPositionOnCircle(10f, 1, 3);
			var last = Angle2dUtility.GetAngleByPositionOnCircle(10f, 2, 3);

			Assert.AreEqual(-10f, first, 0.001f);
			Assert.AreEqual(0f, middle, 0.001f);
			Assert.AreEqual(10f, last, 0.001f);
		}

		[Test]
		public void TestGetAngleByPositionOnCircle_EvenCount_SpacingEqualsOffsetBetweenAllNeighbours()
		{
			var angles = new float[4];
			for (var i = 0; i < 4; i++)
				angles[i] = Angle2dUtility.GetAngleByPositionOnCircle(10f, i, 4);

			Assert.AreEqual(-15f, angles[0], 0.001f);
			Assert.AreEqual(-5f, angles[1], 0.001f);
			Assert.AreEqual(5f, angles[2], 0.001f);
			Assert.AreEqual(15f, angles[3], 0.001f);

			for (var i = 1; i < angles.Length; i++)
				Assert.AreEqual(10f, angles[i] - angles[i - 1], 0.001f);
		}

		[Test]
		public void TestGetAngleByPositionOnCircle_FirstAndLastAreSymmetric()
		{
			var first = Angle2dUtility.GetAngleByPositionOnCircle(7f, 0, 5);
			var last = Angle2dUtility.GetAngleByPositionOnCircle(7f, 4, 5);

			Assert.AreEqual(-first, last, 0.001f);
		}

		[Test]
		public void TestRotateToPositionOnCircle_AppliesComputedAngleAroundLocalZ()
		{
			var go = new GameObject("Test Object");
			var transform = go.transform;

			transform.RotateToPositionOnCircle(10f, 0, 3);

			var expected = Quaternion.Euler(0, 0, -10f);
			Assert.Less(Quaternion.Angle(expected, transform.localRotation), 0.01f);
		}

		[Test]
		public void TestRotateToPositionOnCircle_SingleElement_LeavesRotationUnchanged()
		{
			var go = new GameObject("Test Object");
			var transform = go.transform;

			transform.RotateToPositionOnCircle(10f, 0, 1);

			Assert.AreEqual(Quaternion.identity, transform.localRotation);
		}
	}
}
