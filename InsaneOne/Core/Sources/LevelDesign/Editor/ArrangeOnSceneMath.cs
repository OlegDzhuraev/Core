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

using System;
using UnityEngine;

namespace InsaneOne.Core.LevelDesign
{
	/// <summary> Pure position/rotation calculations behind ArrangeOnSceneWindow's Align/Distribute/Circle/Stack modes,
	/// kept separate from the window so the algorithms can be exercised by tests without going through the
	/// EditorWindow/UI Toolkit machinery. Nothing here touches SessionState, Selection or any UI - every method only
	/// reads Transform positions/rotations, or writes the specific ones documented on it. </summary>
	public static class ArrangeOnSceneMath
	{
		[Flags]
		public enum Axes
		{
			None = 0,
			X = 1,
			Y = 2,
			Z = 4
		}

		public enum AlignAnchor
		{
			Min,
			Center,
			Max,
			Target
		}

		public enum CircleRadiusMode
		{
			Auto,
			Custom
		}

		public enum CircleCenterMode
		{
			Auto,
			Target
		}

		public enum CircleRotationMode
		{
			None,
			LookAtCenter,
			MatchTargetRotation,
			LookAtNext
		}

		public static void AlignAxis(Transform[] transforms, AlignAnchor anchor, Transform alignTarget, int axisIndex)
		{
			var target = GetAlignTarget(transforms, anchor, alignTarget, axisIndex);

			foreach (var t in transforms)
				t.position = SetAxisValue(t.position, axisIndex, target);
		}

		public static float GetAlignTarget(Transform[] transforms, AlignAnchor anchor, Transform alignTarget, int axisIndex)
		{
			// A chosen scene Transform to align to instead of a value derived from the selection - falls back to
			// Center if none is assigned yet (shouldn't normally happen once the user has picked one).
			if (anchor == AlignAnchor.Target)
				return alignTarget ? GetAxisValue(alignTarget.position, axisIndex) : GetAlignTarget(transforms, AlignAnchor.Center, alignTarget, axisIndex);

			var min = GetAxisValue(transforms[0].position, axisIndex);
			var max = min;

			foreach (var t in transforms)
			{
				var value = GetAxisValue(t.position, axisIndex);

				if (value < min) min = value;
				if (value > max) max = value;
			}

			return anchor switch
			{
				AlignAnchor.Min => min,
				AlignAnchor.Max => max,
				_ => (min + max) * 0.5f,
			};
		}

		// Computed as one final position per transform (keyed by the original, unsorted order) rather than applied
		// axis by axis directly on the transforms - each axis spaces independently off the *original* position, so
		// distributing X first doesn't affect how Y or Z get spaced afterward. All axes share one GetArrangeOrder
		// result, same as Stack - see ApplyDistributedAxis for why sorting by this axis' own current values isn't
		// used anymore.
		public static Vector3[] ComputeDistributedPositions(Transform[] transforms, Axes axes)
		{
			var positions = new Vector3[transforms.Length];
			for (var i = 0; i < transforms.Length; i++)
				positions[i] = transforms[i].position;

			var order = GetArrangeOrder(transforms, axes);

			if ((axes & Axes.X) != 0) ApplyDistributedAxis(transforms, positions, order, 0);
			if ((axes & Axes.Y) != 0) ApplyDistributedAxis(transforms, positions, order, 1);
			if ((axes & Axes.Z) != 0) ApplyDistributedAxis(transforms, positions, order, 2);

			return positions;
		}

		// The current min and max values on this axis stay the range's two ends, but which object lands on which
		// step in between now follows the shared GetArrangeOrder (spread on the other axes) instead of this axis'
		// own current values - sorting by the axis being distributed looked arbitrary whenever the selection was
		// already close together on it (the exact bug GetArrangeOrder was written to fix for Stack). The range
		// itself is unchanged - every object still ends up within [min, max], just not necessarily the same object
		// that originally held the min/max value.
		public static void ApplyDistributedAxis(Transform[] transforms, Vector3[] positions, int[] order, int axisIndex)
		{
			var min = GetAxisValue(transforms[0].position, axisIndex);
			var max = min;

			foreach (var t in transforms)
			{
				var value = GetAxisValue(t.position, axisIndex);

				if (value < min) min = value;
				if (value > max) max = value;
			}

			var step = order.Length > 1 ? (max - min) / (order.Length - 1) : 0f;

			for (var rank = 0; rank < order.Length; rank++)
			{
				var index = order[rank];
				positions[index] = SetAxisValue(positions[index], axisIndex, min + step * rank);
			}
		}

		// Shared by Stack and Distribute. Selection.transforms' own order isn't reliable to arrange objects by -
		// Unity doesn't guarantee it follows click order, so using it as-is can look arbitrary (confirmed by
		// feedback: order still looked random after a previous attempt that relied on it, for both tools). Instead,
		// order by position along whichever axis - among the ones NOT being modified - the selection is spread out
		// on the most: that's normally the existing "reading order" objects are laid out in before being arranged
		// (e.g. left-to-right on X, about to be stacked upward on Y, or distributed along Z). Falls back to the
		// transforms' own order only if there's no such axis to go by (e.g. all three axes are being modified at
		// once, or the objects already overlap on every other axis too).
		public static int[] GetArrangeOrder(Transform[] transforms, Axes modifiedAxes)
		{
			var order = new int[transforms.Length];
			for (var i = 0; i < order.Length; i++)
				order[i] = i;

			var sortAxis = -1;
			var bestSpread = 0f;

			for (var axisIndex = 0; axisIndex < 3; axisIndex++)
			{
				if ((modifiedAxes & (Axes) (1 << axisIndex)) != 0)
					continue;

				var min = GetAxisValue(transforms[0].position, axisIndex);
				var max = min;

				foreach (var t in transforms)
				{
					var value = GetAxisValue(t.position, axisIndex);

					if (value < min) min = value;
					if (value > max) max = value;
				}

				var spread = max - min;
				if (spread > bestSpread)
				{
					bestSpread = spread;
					sortAxis = axisIndex;
				}
			}

			if (sortAxis >= 0)
				Array.Sort(order, (a, b) => GetAxisValue(transforms[a].position, sortAxis).CompareTo(GetAxisValue(transforms[b].position, sortAxis)));

			return order;
		}

		public static Vector3[] ComputeStackedPositions(Transform[] transforms, Axes axes, float spacing)
		{
			var positions = new Vector3[transforms.Length];
			for (var i = 0; i < transforms.Length; i++)
				positions[i] = transforms[i].position;

			var order = GetArrangeOrder(transforms, axes);

			if ((axes & Axes.X) != 0) ApplyStackedAxis(transforms, positions, order, 0, spacing);
			if ((axes & Axes.Y) != 0) ApplyStackedAxis(transforms, positions, order, 1, spacing);
			if ((axes & Axes.Z) != 0) ApplyStackedAxis(transforms, positions, order, 2, spacing);

			return positions;
		}

		// The object first in the arrange order is the anchor and keeps its position on this axis; the rest follow
		// at a fixed step in that same order.
		public static void ApplyStackedAxis(Transform[] transforms, Vector3[] positions, int[] order, int axisIndex, float spacing)
		{
			var anchorValue = GetAxisValue(transforms[order[0]].position, axisIndex);

			for (var rank = 0; rank < order.Length; rank++)
			{
				var index = order[rank];
				positions[index] = SetAxisValue(positions[index], axisIndex, anchorValue + spacing * rank);
			}
		}

		// Center is either derived from the selection (average position, keeping this a one-click tool like
		// Align/Distribute) or, if Target is selected, the position of a chosen scene Transform - falls back to the
		// average if none is assigned yet. Radius is either derived from the selection too (average distance from
		// that center) or, if Custom is selected, the value from the Radius field. Which object ends up on which of
		// the evenly spaced slots is decided by AssignNearestSlots - working entirely off the un-rotated ring, so
		// that choice doesn't shift as angleOffset changes (see AssignNearestSlots) - and angleOffset is applied
		// afterwards, turning the whole ring by a fixed amount without re-litigating who stands where.
		public static Vector3[] ComputeCirclePositions(Transform[] transforms, CircleRadiusMode radiusMode, float customRadius, CircleCenterMode centerMode, Transform centerTarget, float angleOffset)
		{
			var center = GetCircleCenter(transforms, centerMode, centerTarget);
			var radius = radiusMode == CircleRadiusMode.Custom ? customRadius : GetAverageRadius(transforms, center);
			var angleStep = 360f / transforms.Length;

			var (order, shift) = AssignNearestSlots(transforms, center, radius, angleStep);

			var positions = new Vector3[transforms.Length];
			for (var rank = 0; rank < order.Length; rank++)
			{
				var slotIndex = (rank + shift) % order.Length;
				var angle = (slotIndex * angleStep + angleOffset) * Mathf.Deg2Rad;
				positions[order[rank]] = new Vector3(center.x + Mathf.Cos(angle) * radius, center.y, center.z + Mathf.Sin(angle) * radius);
			}

			return positions;
		}

		// A plain "closest remaining pair first" greedy match can leave an object stuck with a distant slot just
		// because closer objects claimed the nearby ones first in an order that doesn't actually minimize how far
		// anyone has to move - most noticeable with a Target center far from the selection, where every object
		// wants roughly the same handful of slots. This instead uses the standard trick for assigning points to a
		// convex circle optimally: sort objects by their current angle around the center - any assignment that
		// crosses this order can always be improved by swapping the two crossing objects (a shorter, non-crossing
		// pairing of segments always has less total length), so the true optimum never crosses it - then try every
		// cyclic rotation of which sorted object starts at slot 0 and keep whichever rotation moves the selection
		// the least in total. Deliberately computed off the un-rotated ring (angleOffset is never part of this cost)
		// - otherwise angleOffset would just get "optimized away" back to whichever rotation already fits best,
		// instead of actually turning the ring, and would occasionally cause the whole assignment to jump to a
		// different rotation once a large enough offset made some other rotation marginally cheaper.
		public static (int[] order, int shift) AssignNearestSlots(Transform[] transforms, Vector3 center, float radius, float angleStep)
		{
			var count = transforms.Length;
			var order = new int[count];
			for (var i = 0; i < count; i++)
				order[i] = i;

			Array.Sort(order, (a, b) => GetAngleAroundCenter(transforms[a].position, center).CompareTo(GetAngleAroundCenter(transforms[b].position, center)));

			var bestShift = 0;
			var bestCost = float.MaxValue;

			for (var shift = 0; shift < count; shift++)
			{
				var cost = 0f;
				for (var rank = 0; rank < count; rank++)
				{
					var angle = ((rank + shift) % count) * angleStep * Mathf.Deg2Rad;
					var slot = new Vector3(center.x + Mathf.Cos(angle) * radius, center.y, center.z + Mathf.Sin(angle) * radius);
					cost += (transforms[order[rank]].position - slot).sqrMagnitude;
				}

				if (cost < bestCost)
				{
					bestCost = cost;
					bestShift = shift;
				}
			}

			return (order, bestShift);
		}

		public static float GetAngleAroundCenter(Vector3 position, Vector3 center) => Mathf.Atan2(position.z - center.z, position.x - center.x);

		public static Vector3 GetCircleCenter(Transform[] transforms, CircleCenterMode centerMode, Transform centerTarget) =>
			centerMode == CircleCenterMode.Target && centerTarget ? centerTarget.position : GetAverageCenter(transforms);

		// Applied after the positions are already set, using the final (post-arrange) positions - so LookAtCenter
		// and LookAtNext orient objects based on where they end up on the ring, not where they started.
		public static void ApplyCircleRotations(Transform[] transforms, Vector3[] positions, Vector3 center, CircleRotationMode rotationMode, Transform centerTarget)
		{
			switch (rotationMode)
			{
				case CircleRotationMode.LookAtCenter:
					for (var i = 0; i < transforms.Length; i++)
						LookAt(transforms[i], positions[i], center);
					break;

				case CircleRotationMode.MatchTargetRotation:
					// Same Target the Circle Center field points at - nothing to copy if it isn't assigned, so this
					// silently no-ops rather than picking some other rotation to fall back to.
					if (!centerTarget)
						break;

					foreach (var t in transforms)
						t.rotation = centerTarget.rotation;
					break;

				case CircleRotationMode.LookAtNext:
					ApplyLookAtNextRotations(transforms, positions, center);
					break;
			}
		}

		// Each object faces whichever object comes after it going around the ring - the chain order is read off the
		// final positions' angle around the center (not the internal AssignNearestSlots order), so it always
		// matches the ring as it visually ends up, angleOffset included.
		public static void ApplyLookAtNextRotations(Transform[] transforms, Vector3[] positions, Vector3 center)
		{
			var count = transforms.Length;
			var order = new int[count];
			for (var i = 0; i < count; i++)
				order[i] = i;

			Array.Sort(order, (a, b) => GetAngleAroundCenter(positions[a], center).CompareTo(GetAngleAroundCenter(positions[b], center)));

			for (var rank = 0; rank < count; rank++)
			{
				var current = order[rank];
				var next = order[(rank + 1) % count];
				LookAt(transforms[current], positions[current], positions[next]);
			}
		}

		public static void LookAt(Transform t, Vector3 fromPosition, Vector3 towardsPosition)
		{
			var direction = towardsPosition - fromPosition;
			if (direction.sqrMagnitude > 0.0001f)
				t.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
		}

		public static Vector3 GetAverageCenter(Transform[] transforms)
		{
			var center = Vector3.zero;
			foreach (var t in transforms)
				center += t.position;

			return center / transforms.Length;
		}

		public static float GetAverageRadius(Transform[] transforms, Vector3 center)
		{
			var radius = 0f;
			foreach (var t in transforms)
				radius += Vector2.Distance(new Vector2(t.position.x, t.position.z), new Vector2(center.x, center.z));

			return radius / transforms.Length;
		}

		public static float GetAxisValue(Vector3 v, int axisIndex) => axisIndex switch { 0 => v.x, 1 => v.y, _ => v.z };

		public static Vector3 SetAxisValue(Vector3 v, int axisIndex, float value)
		{
			switch (axisIndex)
			{
				case 0:
					v.x = value;
					break;
				case 1:
					v.y = value;
					break;
				default:
					v.z = value;
					break;
			}

			return v;
		}
	}
}
