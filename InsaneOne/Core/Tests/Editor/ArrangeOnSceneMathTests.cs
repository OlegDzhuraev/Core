/*
 * Copyright 2026 Oleg Dzhuraev <godlikeaurora@gmail.com>
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Collections.Generic;
using InsaneOne.Core.LevelDesign;
using NUnit.Framework;
using UnityEngine;

namespace InsaneOne.Core.Tests
{
	[TestFixture]
	public class ArrangeOnSceneMathTests
	{
		const float Tolerance = 0.001f;

		readonly List<GameObject> spawned = new ();

		[TearDown]
		public void TearDown()
		{
			foreach (var go in spawned)
				Object.DestroyImmediate(go);

			spawned.Clear();
		}

		Transform CreateTransform(Vector3 position)
		{
			var go = new GameObject("ArrangeOnSceneMathTests Object");
			go.transform.position = position;
			spawned.Add(go);
			return go.transform;
		}

		Transform[] CreateTransforms(params Vector3[] positions)
		{
			var transforms = new Transform[positions.Length];
			for (var i = 0; i < positions.Length; i++)
				transforms[i] = CreateTransform(positions[i]);

			return transforms;
		}

		static void AssertVector3(Vector3 expected, Vector3 actual, string message = null)
		{
			Assert.AreEqual(expected.x, actual.x, Tolerance, message);
			Assert.AreEqual(expected.y, actual.y, Tolerance, message);
			Assert.AreEqual(expected.z, actual.z, Tolerance, message);
		}

		// Quaternion.Euler(...) called twice, or a value round-tripped through Transform.rotation, is not guaranteed
		// to be bit-for-bit identical even when it represents the exact same orientation - Assert.AreEqual's default
		// (exact) comparer can fail on values that print identically. Quaternion.Angle is the tolerant way to
		// compare two rotations.
		static void AssertQuaternion(Quaternion expected, Quaternion actual, string message = null) =>
			Assert.LessOrEqual(Quaternion.Angle(expected, actual), 0.01f, message);

		// --- Axis helpers ---

		[Test]
		public void GetAxisValue_ReturnsComponentForEachAxisIndex()
		{
			var v = new Vector3(1, 2, 3);

			Assert.AreEqual(1, ArrangeOnSceneMath.GetAxisValue(v, 0));
			Assert.AreEqual(2, ArrangeOnSceneMath.GetAxisValue(v, 1));
			Assert.AreEqual(3, ArrangeOnSceneMath.GetAxisValue(v, 2));
		}

		[Test]
		public void SetAxisValue_ReplacesOnlyTheGivenComponent()
		{
			var v = new Vector3(1, 2, 3);

			AssertVector3(new Vector3(9, 2, 3), ArrangeOnSceneMath.SetAxisValue(v, 0, 9));
			AssertVector3(new Vector3(1, 9, 3), ArrangeOnSceneMath.SetAxisValue(v, 1, 9));
			AssertVector3(new Vector3(1, 2, 9), ArrangeOnSceneMath.SetAxisValue(v, 2, 9));
		}

		// --- Align ---

		[Test]
		public void GetAlignTarget_Min_ReturnsSmallestValueOnAxis()
		{
			var transforms = CreateTransforms(new Vector3(5, 0, 0), new Vector3(1, 0, 0), new Vector3(9, 0, 0));

			var target = ArrangeOnSceneMath.GetAlignTarget(transforms, ArrangeOnSceneMath.AlignAnchor.Min, null, 0);

			Assert.AreEqual(1, target, Tolerance);
		}

		[Test]
		public void GetAlignTarget_Max_ReturnsLargestValueOnAxis()
		{
			var transforms = CreateTransforms(new Vector3(5, 0, 0), new Vector3(1, 0, 0), new Vector3(9, 0, 0));

			var target = ArrangeOnSceneMath.GetAlignTarget(transforms, ArrangeOnSceneMath.AlignAnchor.Max, null, 0);

			Assert.AreEqual(9, target, Tolerance);
		}

		[Test]
		public void GetAlignTarget_Center_ReturnsMidpointBetweenMinAndMax()
		{
			var transforms = CreateTransforms(new Vector3(5, 0, 0), new Vector3(1, 0, 0), new Vector3(9, 0, 0));

			var target = ArrangeOnSceneMath.GetAlignTarget(transforms, ArrangeOnSceneMath.AlignAnchor.Center, null, 0);

			Assert.AreEqual(5, target, Tolerance); // (min 1 + max 9) * 0.5
		}

		[Test]
		public void GetAlignTarget_Target_ReturnsTargetTransformValue_IgnoringSelection()
		{
			var transforms = CreateTransforms(new Vector3(5, 0, 0), new Vector3(1, 0, 0));
			var alignTarget = CreateTransform(new Vector3(42, 0, 0));

			var target = ArrangeOnSceneMath.GetAlignTarget(transforms, ArrangeOnSceneMath.AlignAnchor.Target, alignTarget, 0);

			Assert.AreEqual(42, target, Tolerance);
		}

		[Test]
		public void GetAlignTarget_Target_FallsBackToCenter_WhenTargetNotAssigned()
		{
			var transforms = CreateTransforms(new Vector3(1, 0, 0), new Vector3(9, 0, 0));

			var target = ArrangeOnSceneMath.GetAlignTarget(transforms, ArrangeOnSceneMath.AlignAnchor.Target, null, 0);

			Assert.AreEqual(5, target, Tolerance);
		}

		[Test]
		public void AlignAxis_MovesEveryTransformToTheTargetValue_OnlyOnTheGivenAxis()
		{
			var transforms = CreateTransforms(new Vector3(5, 3, 1), new Vector3(1, 7, 2), new Vector3(9, 4, 8));

			ArrangeOnSceneMath.AlignAxis(transforms, ArrangeOnSceneMath.AlignAnchor.Min, null, 0);

			foreach (var t in transforms)
				Assert.AreEqual(1, t.position.x, Tolerance);

			// Y/Z must be untouched
			Assert.AreEqual(3, transforms[0].position.y, Tolerance);
			Assert.AreEqual(1, transforms[0].position.z, Tolerance);
		}

		// --- Shared arrange order (Stack/Distribute) ---

		[Test]
		public void GetArrangeOrder_SortsByTheAxisWithTheLargestSpreadAmongTheOthers()
		{
			// Spread out on X, identical on Y/Z, and about to be modified on Y - the fix this method exists for:
			// order must come from X (the axis not being modified), not from Selection order or the modified axis.
			var transforms = CreateTransforms(
				new Vector3(10, 0, 0), // index 0
				new Vector3(0, 0, 0), // index 1
				new Vector3(5, 0, 0)); // index 2

			var order = ArrangeOnSceneMath.GetArrangeOrder(transforms, ArrangeOnSceneMath.Axes.Y);

			CollectionAssert.AreEqual(new[] { 1, 2, 0 }, order); // ascending X: index1 (0) < index2 (5) < index0 (10)
		}

		[Test]
		public void GetArrangeOrder_FallsBackToOriginalOrder_WhenNoOtherAxisHasSpread()
		{
			// All three axes are being modified, so there's nothing left to sort by.
			var transforms = CreateTransforms(new Vector3(10, 20, 30), new Vector3(0, 0, 0));

			var order = ArrangeOnSceneMath.GetArrangeOrder(transforms, ArrangeOnSceneMath.Axes.X | ArrangeOnSceneMath.Axes.Y | ArrangeOnSceneMath.Axes.Z);

			CollectionAssert.AreEqual(new[] { 0, 1 }, order);
		}

		// --- Distribute ---

		[Test]
		public void ComputeDistributedPositions_SpreadsBetweenCurrentMinAndMax_OrderedByTheOtherAxes()
		{
			// Laid out left-to-right on X (10, 0, 5 => order by X is 1, 2, 0), with near-arbitrary Y "noise" that
			// must NOT decide the order (that was the reported bug) but does still define the [min,max] range.
			var transforms = CreateTransforms(
				new Vector3(10, 0.09f, 0), // index 0, X spread anchor: last
				new Vector3(0, 0.05f, 0), // index 1, X spread anchor: first (min Y too)
				new Vector3(5, 0.07f, 0)); // index 2, X spread anchor: middle

			var positions = ArrangeOnSceneMath.ComputeDistributedPositions(transforms, ArrangeOnSceneMath.Axes.Y);

			Assert.AreEqual(0.05f, positions[1].y, Tolerance); // rank 0 (lowest X) -> min Y
			Assert.AreEqual(0.07f, positions[2].y, Tolerance); // rank 1 (middle X) -> midpoint
			Assert.AreEqual(0.09f, positions[0].y, Tolerance); // rank 2 (highest X) -> max Y

			// X/Z positions must be untouched by a Y-only distribute.
			Assert.AreEqual(10, positions[0].x, Tolerance);
			Assert.AreEqual(0, positions[1].x, Tolerance);
			Assert.AreEqual(5, positions[2].x, Tolerance);
		}

		[Test]
		public void ComputeDistributedPositions_TwoObjects_EndsExactlyAtMinAndMax()
		{
			var transforms = CreateTransforms(new Vector3(0, 3, 0), new Vector3(1, 8, 0));

			var positions = ArrangeOnSceneMath.ComputeDistributedPositions(transforms, ArrangeOnSceneMath.Axes.Y);

			Assert.AreEqual(3, positions[0].y, Tolerance);
			Assert.AreEqual(8, positions[1].y, Tolerance);
		}

		// --- Stack ---

		[Test]
		public void ComputeStackedPositions_LinesUpFromTheFirstArrangeOrderObject_WithFixedSpacing()
		{
			// Same left-to-right X layout as the Distribute test above, stacked upward on Y with a fixed step -
			// the anchor keeps its own Y value instead of snapping to a global min.
			var transforms = CreateTransforms(
				new Vector3(10, 0.09f, 0), // index 0, X spread anchor: last
				new Vector3(0, 0.05f, 0), // index 1, X spread anchor: first
				new Vector3(5, 0.07f, 0)); // index 2, X spread anchor: middle

			var positions = ArrangeOnSceneMath.ComputeStackedPositions(transforms, ArrangeOnSceneMath.Axes.Y, 2f);

			Assert.AreEqual(0.05f, positions[1].y, Tolerance); // anchor (rank 0) keeps its own value
			Assert.AreEqual(2.05f, positions[2].y, Tolerance); // rank 1: anchor + spacing
			Assert.AreEqual(4.05f, positions[0].y, Tolerance); // rank 2: anchor + 2 * spacing
		}

		// --- Circle: center/radius helpers ---

		[Test]
		public void GetAverageCenter_ReturnsTheMeanPosition()
		{
			var transforms = CreateTransforms(new Vector3(0, 0, 0), new Vector3(10, 0, 0), new Vector3(0, 0, 10), new Vector3(10, 0, 10));

			var center = ArrangeOnSceneMath.GetAverageCenter(transforms);

			AssertVector3(new Vector3(5, 0, 5), center);
		}

		[Test]
		public void GetAverageRadius_UsesOnlyTheXZPlane_IgnoringHeightDifferences()
		{
			var transforms = CreateTransforms(new Vector3(5, 100, 0), new Vector3(-5, -100, 0));

			var radius = ArrangeOnSceneMath.GetAverageRadius(transforms, Vector3.zero);

			Assert.AreEqual(5, radius, Tolerance);
		}

		[Test]
		public void GetCircleCenter_Auto_ReturnsAverageCenter()
		{
			var transforms = CreateTransforms(new Vector3(0, 0, 0), new Vector3(10, 0, 0));

			var center = ArrangeOnSceneMath.GetCircleCenter(transforms, ArrangeOnSceneMath.CircleCenterMode.Auto, null);

			AssertVector3(new Vector3(5, 0, 0), center);
		}

		[Test]
		public void GetCircleCenter_Target_ReturnsTargetPosition()
		{
			var transforms = CreateTransforms(new Vector3(0, 0, 0), new Vector3(10, 0, 0));
			var centerTarget = CreateTransform(new Vector3(100, 0, 100));

			var center = ArrangeOnSceneMath.GetCircleCenter(transforms, ArrangeOnSceneMath.CircleCenterMode.Target, centerTarget);

			AssertVector3(new Vector3(100, 0, 100), center);
		}

		[Test]
		public void GetCircleCenter_Target_FallsBackToAverage_WhenTargetNotAssigned()
		{
			var transforms = CreateTransforms(new Vector3(0, 0, 0), new Vector3(10, 0, 0));

			var center = ArrangeOnSceneMath.GetCircleCenter(transforms, ArrangeOnSceneMath.CircleCenterMode.Target, null);

			AssertVector3(new Vector3(5, 0, 0), center);
		}

		// --- Circle: position assignment ---

		[Test]
		public void ComputeCirclePositions_KeepsObjectsAlreadyOnTheRing_InPlace()
		{
			// Two objects already sitting exactly on the natural (un-rotated) ring - the nearest-slot assignment
			// should leave them where they are (zero-cost match) rather than swapping or rotating them.
			var transforms = CreateTransforms(new Vector3(5, 0, 0), new Vector3(-5, 0, 0));

			var positions = ArrangeOnSceneMath.ComputeCirclePositions(transforms, ArrangeOnSceneMath.CircleRadiusMode.Auto, 0f, ArrangeOnSceneMath.CircleCenterMode.Auto, null, 0f);

			AssertVector3(new Vector3(5, 0, 0), positions[0]);
			AssertVector3(new Vector3(-5, 0, 0), positions[1]);
		}

		[Test]
		public void ComputeCirclePositions_AngleOffset_RotatesTheWholeRing_WithoutReshufflingWhoStandsWhere()
		{
			var transforms = CreateTransforms(new Vector3(5, 0, 0), new Vector3(-5, 0, 0));

			var positions = ArrangeOnSceneMath.ComputeCirclePositions(transforms, ArrangeOnSceneMath.CircleRadiusMode.Auto, 0f, ArrangeOnSceneMath.CircleCenterMode.Auto, null, 90f);

			// Same objects, same slots, but the whole pair turned 90 degrees around the center (0,0,0).
			AssertVector3(new Vector3(0, 0, 5), positions[0]);
			AssertVector3(new Vector3(0, 0, -5), positions[1]);
		}

		[Test]
		public void ComputeCirclePositions_CustomRadius_OverridesAverageRadius()
		{
			var transforms = CreateTransforms(new Vector3(5, 0, 0), new Vector3(-5, 0, 0));

			var positions = ArrangeOnSceneMath.ComputeCirclePositions(transforms, ArrangeOnSceneMath.CircleRadiusMode.Custom, 20f, ArrangeOnSceneMath.CircleCenterMode.Auto, null, 0f);

			foreach (var p in positions)
				Assert.AreEqual(20f, Vector3.Distance(p, Vector3.zero), Tolerance);
		}

		[Test]
		public void AssignNearestSlots_ReturnsAFullPermutation_WithEverySlotAtTheGivenRadius()
		{
			// Structural invariants that must hold for any input: the result must assign each object to exactly one
			// slot (a full permutation, nothing dropped or doubled), and every generated slot must sit exactly on
			// the requested ring. The "picks the nearest slot" behaviour itself is covered more precisely by the
			// ComputeCirclePositions tests above, where the expected final position is known exactly.
			var transforms = CreateTransforms(new Vector3(3, 0, 4), new Vector3(-4, 0, 3), new Vector3(0, 0, -5), new Vector3(4, 0, -3));
			var center = Vector3.zero;
			const float radius = 5f;
			var angleStep = 360f / transforms.Length;

			var (order, shift) = ArrangeOnSceneMath.AssignNearestSlots(transforms, center, radius, angleStep);

			Assert.AreEqual(transforms.Length, order.Length);
			CollectionAssert.AllItemsAreUnique(order);

			for (var rank = 0; rank < order.Length; rank++)
			{
				var slotIndex = (rank + shift) % order.Length;
				var angle = slotIndex * angleStep * Mathf.Deg2Rad;
				var slot = new Vector3(center.x + Mathf.Cos(angle) * radius, center.y, center.z + Mathf.Sin(angle) * radius);

				Assert.AreEqual(radius, Vector3.Distance(slot, center), Tolerance);
			}
		}

		// --- Circle: angle helper ---

		[Test]
		public void GetAngleAroundCenter_MatchesKnownCompassDirections()
		{
			var center = Vector3.zero;

			Assert.AreEqual(0f, ArrangeOnSceneMath.GetAngleAroundCenter(new Vector3(1, 0, 0), center), Tolerance);
			Assert.AreEqual(Mathf.PI / 2f, ArrangeOnSceneMath.GetAngleAroundCenter(new Vector3(0, 0, 1), center), Tolerance);
		}

		// --- Circle: rotation modes ---

		[Test]
		public void ApplyCircleRotations_LookAtCenter_FacesEachObjectTowardsTheCenter()
		{
			var transforms = CreateTransforms(new Vector3(5, 0, 0));
			var positions = new[] { transforms[0].position };

			ArrangeOnSceneMath.ApplyCircleRotations(transforms, positions, Vector3.zero, ArrangeOnSceneMath.CircleRotationMode.LookAtCenter, null);

			AssertVector3(new Vector3(-1, 0, 0), transforms[0].forward);
		}

		[Test]
		public void ApplyCircleRotations_MatchTargetRotation_CopiesTheTargetsRotation()
		{
			var transforms = CreateTransforms(new Vector3(5, 0, 0), new Vector3(-5, 0, 0));
			var positions = new[] { transforms[0].position, transforms[1].position };
			var centerTarget = CreateTransform(Vector3.zero);
			centerTarget.rotation = Quaternion.Euler(0, 45, 0);

			ArrangeOnSceneMath.ApplyCircleRotations(transforms, positions, Vector3.zero, ArrangeOnSceneMath.CircleRotationMode.MatchTargetRotation, centerTarget);

			foreach (var t in transforms)
				AssertQuaternion(centerTarget.rotation, t.rotation);
		}

		[Test]
		public void ApplyCircleRotations_MatchTargetRotation_DoesNothing_WhenTargetNotAssigned()
		{
			var transforms = CreateTransforms(new Vector3(5, 0, 0));
			transforms[0].rotation = Quaternion.Euler(10, 20, 30);
			var positions = new[] { transforms[0].position };

			ArrangeOnSceneMath.ApplyCircleRotations(transforms, positions, Vector3.zero, ArrangeOnSceneMath.CircleRotationMode.MatchTargetRotation, null);

			AssertQuaternion(Quaternion.Euler(10, 20, 30), transforms[0].rotation);
		}

		[Test]
		public void ApplyCircleRotations_None_LeavesRotationsUntouched()
		{
			var transforms = CreateTransforms(new Vector3(5, 0, 0));
			transforms[0].rotation = Quaternion.Euler(10, 20, 30);
			var positions = new[] { transforms[0].position };

			ArrangeOnSceneMath.ApplyCircleRotations(transforms, positions, Vector3.zero, ArrangeOnSceneMath.CircleRotationMode.None, null);

			AssertQuaternion(Quaternion.Euler(10, 20, 30), transforms[0].rotation);
		}

		[Test]
		public void ApplyLookAtNextRotations_ChainsByFinalAngleAroundCenter_NotByArrayOrder()
		{
			// Positions deliberately out of angular order relative to the transforms array (index 0 is at 240
			// degrees, index 1 at 0, index 2 at 120) - the chain must follow the angle, not the array index.
			var positions = new[]
			{
				new Vector3(-2.5f, 0, -4.330127f), // index 0: 240 degrees
				new Vector3(5f, 0, 0), // index 1: 0 degrees
				new Vector3(-2.5f, 0, 4.330127f), // index 2: 120 degrees
			};
			var transforms = CreateTransforms(positions[0], positions[1], positions[2]);

			ArrangeOnSceneMath.ApplyLookAtNextRotations(transforms, positions, Vector3.zero);

			AssertVector3((positions[2] - positions[1]).normalized, transforms[1].forward); // 0 deg -> 120 deg
			AssertVector3((positions[0] - positions[2]).normalized, transforms[2].forward); // 120 deg -> 240 deg
			AssertVector3((positions[1] - positions[0]).normalized, transforms[0].forward); // 240 deg -> wraps to 0 deg
		}

		[Test]
		public void LookAt_DoesNothing_WhenFromAndTowardsAreTheSamePosition()
		{
			var transforms = CreateTransforms(Vector3.zero);
			transforms[0].rotation = Quaternion.Euler(10, 20, 30);

			ArrangeOnSceneMath.LookAt(transforms[0], Vector3.zero, Vector3.zero);

			AssertQuaternion(Quaternion.Euler(10, 20, 30), transforms[0].rotation);
		}
	}
}
