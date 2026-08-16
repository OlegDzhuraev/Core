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
using System.Collections.Generic;
using UnityEngine;

namespace InsaneOne.Core
{
	///<summary> Draws wire-gizmo with selected color and form. Useful to show primary objects in editor when them isn't selected.</summary>
	[DisallowMultipleComponent]
	public class GizmoHandle : MonoBehaviour
	{
		#region Enumerations

		enum Form
		{
			Box,
			Sphere,
			Cylinder,
			Capsule,
			Cross,
			Arrow
		}

		enum DrawType
		{
			Wired,
			WiredAndFilled
		}

		enum Visibility
		{
			Always,
			OnlySelected,
			OnlyWhenNotSelected
		}

		enum RadiusMode
		{
			None,
			Sphere,
			Circle
		}

		enum LinkStyle
		{
			Arrow,
			Line
		}

		#endregion

		#region Constants

		const string BuiltinSphereMeshPath = "Sphere.fbx";
		const string BuiltinCylinderMeshPath = "Cylinder.fbx";
		const string BuiltinCapsuleMeshPath = "Capsule.fbx";

		const int CircleSegments = 24;
		const float ArrowHeadAngle = 25f;
		const float ArrowHeadLength = 0.2f;
		const float RadiusFillAlpha = 0.15f;

		// Measured from the built-in mesh's own bounds rather than assumed, so the filled mesh always matches the wireframe exactly.
		// Also used instead of Gizmos.DrawSphere, whose built-in filled sphere is visibly low-poly/faceted.
		static Mesh sphereMesh;
		static Vector3 sphereMeshExtents;
		static Mesh cylinderMesh;
		static Vector3 cylinderMeshExtents;
		static Mesh capsuleMesh;
		static Vector3 capsuleMeshExtents;

		#endregion

		/// <summary> Handles this one is currently linked to (see the Links section) - draws a connection to each of them. </summary>
		public IReadOnlyList<GizmoHandle> LinkedTo => linkedTo;

		[SerializeField] Color gizmoColor = Color.red;
		[SerializeField] Form form = Form.Box;
		[Tooltip("WiredAndFilled only has effect for the Box, Sphere, Cylinder and Capsule forms - Cross and Arrow are always wire-only.")]
		[SerializeField] DrawType drawType = DrawType.Wired;
		[SerializeField] Visibility visibility = Visibility.Always;

		[Header("Radius")]
		[Tooltip("Draws an extra sphere around the object - useful to mark a trigger/spawn/detection zone, independent of the main form above.\nCircle squashes the radius sphere to zero height (Y = 0), drawing it as a flat disc - useful for ground-level 2D zones.")]
		[SerializeField] RadiusMode radiusMode = RadiusMode.None;

		[Tooltip("Draws an extra sphere around the object - useful to mark a trigger/spawn/detection zone, independent of the main form above.")]
		[SerializeField, Min(0f)] float radius = 2f;
		[SerializeField] Color radiusColor = Color.white;

		[Header("Links")]
		[Tooltip("Draws a connection from this object to each linked handle - useful for patrol paths, event chains, trigger sequences.")]
		[SerializeField] GizmoHandle[] linkedTo = Array.Empty<GizmoHandle>();
		[SerializeField] LinkStyle linkStyle = LinkStyle.Arrow;
		[SerializeField] Color linkColor = Color.yellow;

		[Header("Label")]
		[SerializeField] bool showLabel;
		[Tooltip("Text shown above the object when Show Label is on. Leave empty to show the GameObject's name instead.")]
		[SerializeField] string labelText;

		public void SetGizmoColor(Color color) => gizmoColor = color;

		public void SetLinkColor(Color color) => linkColor = color;

		public void SetRadiusColor(Color color) => radiusColor = color;
		public void SetRadius(float value) => radius = value;

		/// <summary> Replaces the full set of linked handles. Nulls are ignored. </summary>
		public void SetLinks(IReadOnlyList<GizmoHandle> links)
		{
			var result = new List<GizmoHandle>(links?.Count ?? 0);

			if (links != null)
			{
				foreach (var link in links)
				{
					if (link)
						result.Add(link);
				}
			}

			linkedTo = result.ToArray();
		}

		/// <summary> Adds a single link, ignoring nulls and ones already linked. </summary>
		public void AddLink(GizmoHandle link)
		{
			if (!link || Array.IndexOf(linkedTo, link) >= 0)
				return;

			Array.Resize(ref linkedTo, linkedTo.Length + 1);
			linkedTo[^1] = link;
		}

		/// <summary> Removes a single link, if it's currently linked. </summary>
		public void RemoveLink(GizmoHandle link)
		{
			var index = Array.IndexOf(linkedTo, link);
			if (index < 0)
				return;

			var result = new List<GizmoHandle>(linkedTo);
			result.RemoveAt(index);
			linkedTo = result.ToArray();
		}

		/// <summary> Removes all links. </summary>
		public void ClearLinks() => linkedTo = Array.Empty<GizmoHandle>();

		void OnDrawGizmos()
		{
			if (visibility is Visibility.OnlySelected)
				return;

#if UNITY_EDITOR
			if (visibility is Visibility.OnlyWhenNotSelected && UnityEditor.Selection.activeGameObject == gameObject)
				return;
#endif

			Draw();
		}

		void OnDrawGizmosSelected()
		{
			if (visibility != Visibility.OnlySelected)
				return;

			Draw();
		}

		void Draw()
		{
			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.color = gizmoColor;

			switch (form)
			{
				case Form.Sphere: DrawSphere(drawType); break;
				case Form.Cylinder: DrawCylinder(drawType); break;
				case Form.Capsule: DrawCapsule(drawType); break;
				case Form.Cross: DrawCross(); break;
				case Form.Arrow: DrawArrow(); break;
				default: DrawCube(drawType); break;
			}

			if (radiusMode is not RadiusMode.None)
				DrawRadius(drawType, radius);

			if (linkedTo != null && linkedTo.Length > 0)
				DrawLinks();

#if UNITY_EDITOR
			if (showLabel)
			{
				var text = string.IsNullOrEmpty(labelText) ? gameObject.name : labelText;
				var style = new GUIStyle { normal = { textColor = gizmoColor } };
				UnityEditor.Handles.Label(transform.position, text, style);
			}
#endif
		}

		void DrawRadius(DrawType type, float value)
		{
			if (value <= 0f)
				return;

			var previousMatrix = Gizmos.matrix;
			if (radiusMode is RadiusMode.Circle)
				Gizmos.matrix *= Matrix4x4.Scale(new Vector3(1f, 0f, 1f)); // squash to Y = 0 in local space, before the transform matrix is applied

			if (type is DrawType.WiredAndFilled)
			{
				Gizmos.color = radiusColor.GetWithAlpha(RadiusFillAlpha);
				DrawFilledSphere(value);
			}

			Gizmos.color = radiusColor;
			Gizmos.DrawWireSphere(Vector3.zero, value);

			Gizmos.matrix = previousMatrix;
		}

		void DrawLinks()
		{
			var previousMatrix = Gizmos.matrix;
			Gizmos.matrix = Matrix4x4.identity; // links connect world-space points between different objects, not this object's local geometry
			Gizmos.color = linkColor;

			var from = transform.position;

			foreach (var link in linkedTo)
			{
				if (!link || link == this)
					continue;

				var to = link.transform.position;

				if (linkStyle is LinkStyle.Arrow)
					DrawLinkArrow(from, to);
				else
					Gizmos.DrawLine(from, to);
			}

			Gizmos.matrix = previousMatrix;
		}

		void DrawLinkArrow(Vector3 from, Vector3 to)
		{
			Gizmos.DrawLine(from, to);

			var direction = to - from;
			if (direction.sqrMagnitude < 0.0001f)
				return;

			direction.Normalize();

			var side = Vector3.Cross(direction, Vector3.up);
			if (side.sqrMagnitude < 0.0001f)
				side = Vector3.Cross(direction, Vector3.right);
			side.Normalize();

			var up = Vector3.Cross(side, direction).normalized;

			DrawArrowHead(to, direction, side);
			DrawArrowHead(to, direction, up);
		}

		void DrawSphere(DrawType type)
		{
			var center = Vector3.zero;
			var radius = 0.5f;

			if (type is DrawType.WiredAndFilled)
				DrawFilledSphere(radius);

			Gizmos.DrawWireSphere(center, radius);
		}

		void DrawCube(DrawType type)
		{
			var center = Vector3.zero;
			var size = Vector3.one;

			if (type is DrawType.WiredAndFilled)
				Gizmos.DrawCube(center, size);

			Gizmos.DrawWireCube(center, size);
		}

		void DrawCylinder(DrawType type)
		{
			const float radius = 0.5f;
			const float halfHeight = 0.5f;

			var mesh = GetCylinderMesh();

			// Non-uniform scale keeps a cylinder's circular cross-section intact, so filled/wire always match.
			if (type is DrawType.WiredAndFilled && mesh && cylinderMeshExtents.x > 0f && cylinderMeshExtents.y > 0f && cylinderMeshExtents.z > 0f)
				DrawFilledPrimitive(mesh, new Vector3(radius / cylinderMeshExtents.x, halfHeight / cylinderMeshExtents.y, radius / cylinderMeshExtents.z));

			var top = new Vector3(0f, halfHeight, 0f);
			var bottom = new Vector3(0f, -halfHeight, 0f);

			DrawCircle(top, radius);
			DrawCircle(bottom, radius);
			DrawSideLines(top, bottom, radius);
		}

		void DrawCapsule(DrawType type)
		{
			const float radius = 0.3f;

			var mesh = GetCapsuleMesh();

			// Uniform scale keeps the hemispherical caps round; the pole offset then follows the built-in mesh's own proportions.
			var hasMesh = mesh && capsuleMeshExtents.x > 0f;
			var scaleFactor = hasMesh ? radius / capsuleMeshExtents.x : 1f;
			var poleOffset = hasMesh ? (capsuleMeshExtents.y - capsuleMeshExtents.x) * scaleFactor : radius;

			if (type is DrawType.WiredAndFilled && hasMesh)
				DrawFilledPrimitive(mesh, Vector3.one * scaleFactor);

			var top = new Vector3(0f, poleOffset, 0f);
			var bottom = new Vector3(0f, -poleOffset, 0f);

			Gizmos.DrawWireSphere(top, radius);
			Gizmos.DrawWireSphere(bottom, radius);
			DrawSideLines(top, bottom, radius);
		}

		void DrawFilledPrimitive(Mesh mesh, Vector3 scale) => Gizmos.DrawMesh(mesh, Vector3.zero, Quaternion.identity, scale);

		void DrawFilledSphere(float radiusValue)
		{
			var mesh = GetSphereMesh();

			if (!mesh || sphereMeshExtents.x <= 0f)
			{
				Gizmos.DrawSphere(Vector3.zero, radiusValue); // fallback, in case the built-in mesh couldn't be loaded
				return;
			}

			DrawFilledPrimitive(mesh, Vector3.one * (radiusValue / sphereMeshExtents.x));
		}

		void DrawCross()
		{
			const float armLength = 0.5f;

			Gizmos.DrawLine(new Vector3(-armLength, 0f, 0f), new Vector3(armLength, 0f, 0f));
			Gizmos.DrawLine(new Vector3(0f, -armLength, 0f), new Vector3(0f, armLength, 0f));
			Gizmos.DrawLine(new Vector3(0f, 0f, -armLength), new Vector3(0f, 0f, armLength));
		}

		void DrawArrow()
		{
			const float length = 1f;

			var tip = Vector3.forward * length;
			Gizmos.DrawLine(Vector3.zero, tip);

			DrawArrowHead(tip, Vector3.forward, Vector3.up);
			DrawArrowHead(tip, Vector3.forward, Vector3.right);
		}

		void DrawArrowHead(Vector3 tip, Vector3 forward, Vector3 planeAxis)
		{
			var left = Quaternion.AngleAxis(180f - ArrowHeadAngle, planeAxis) * forward;
			var right = Quaternion.AngleAxis(180f + ArrowHeadAngle, planeAxis) * forward;

			Gizmos.DrawLine(tip, tip + left * ArrowHeadLength);
			Gizmos.DrawLine(tip, tip + right * ArrowHeadLength);
		}

		void DrawCircle(Vector3 center, float radius)
		{
			var prev = center + new Vector3(radius, 0f, 0f);

			for (var i = 1; i <= CircleSegments; i++)
			{
				var angle = i / (float) CircleSegments * Mathf.PI * 2f;
				var point = center + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
				Gizmos.DrawLine(prev, point);
				prev = point;
			}
		}

		void DrawSideLines(Vector3 top, Vector3 bottom, float radius, int count = 4)
		{
			for (var i = 0; i < count; i++)
			{
				var angle = i / (float) count * Mathf.PI * 2f;
				var offset = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
				Gizmos.DrawLine(bottom + offset, top + offset);
			}
		}

		#region Static mesh helpers

		static Mesh GetSphereMesh()
		{
			if (!sphereMesh)
			{
				sphereMesh = Resources.GetBuiltinResource<Mesh>(BuiltinSphereMeshPath);
				sphereMeshExtents = sphereMesh ? sphereMesh.bounds.extents : Vector3.zero;
			}

			return sphereMesh;
		}

		static Mesh GetCylinderMesh()
		{
			if (!cylinderMesh)
			{
				cylinderMesh = Resources.GetBuiltinResource<Mesh>(BuiltinCylinderMeshPath);
				cylinderMeshExtents = cylinderMesh ? cylinderMesh.bounds.extents : Vector3.zero;
			}

			return cylinderMesh;
		}

		static Mesh GetCapsuleMesh()
		{
			if (!capsuleMesh)
			{
				capsuleMesh = Resources.GetBuiltinResource<Mesh>(BuiltinCapsuleMeshPath);
				capsuleMeshExtents = capsuleMesh ? capsuleMesh.bounds.extents : Vector3.zero;
			}

			return capsuleMesh;
		}

		#endregion
	}
}
