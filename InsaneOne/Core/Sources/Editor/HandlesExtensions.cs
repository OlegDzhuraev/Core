using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace InsaneOne.Core.Development
{
	public static class HandlesExtensions
	{
		public enum SphereViewMode { Flat, Wireframe, FlatWireframe }

		public static CompareFunction DefaultZTest { get; set; } = CompareFunction.Less;
		public static Color DefaultFullColor { get; set; } = new (1f, 1f, 1f, 0.5f);

		static readonly Vector3[] tempPoly = new Vector3[4];
		static readonly Vector3[] tempBoxVerts = new Vector3[8];
		static readonly List<Vector3[]> tempBoxSides = new (6);

		public static void DrawSphere(Vector3 point, float radius, int numSegments = 20, SphereViewMode mode = SphereViewMode.Flat)
		{
			var faceColor = mode is SphereViewMode.Wireframe ? Color.clear : DefaultFullColor;
			var outlineColor = mode is SphereViewMode.Flat ? Color.clear : DefaultFullColor;

			var prevZTest = Handles.zTest;
			Handles.zTest = DefaultZTest;

			for (int i = 0; i < numSegments; i++)
			{
				for (int j = 0; j < numSegments; j++)
				{
					var u1 = (float)i / numSegments;
					var u2 = (float)(i + 1) / numSegments;
					var v1 = (float)j / numSegments;
					var v2 = (float)(j + 1) / numSegments;

					tempPoly[0] = GetPointOnSphere(point, radius, u1, v1);
					tempPoly[1] = GetPointOnSphere(point, radius, u2, v1);
					tempPoly[2] = GetPointOnSphere(point, radius, u2, v2);
					tempPoly[3] = GetPointOnSphere(point, radius, u1, v2);

					Handles.DrawSolidRectangleWithOutline(tempPoly, faceColor, outlineColor);
				}
			}

			Handles.zTest = prevZTest;
		}

		public static void DrawBox(Vector3 point, Vector3 size, SphereViewMode mode = SphereViewMode.Flat)
		{
			var faceColor = mode is SphereViewMode.Wireframe ? Color.clear : DefaultFullColor;
			var outlineColor = mode is SphereViewMode.Flat ? Color.clear : DefaultFullColor;
			
			var hs = new Vector3(size.x / 2, size.y / 2, size.z / 2);

			var verts = tempBoxVerts; // just caching with a smaller name

			verts[0] = point + new Vector3(hs.x, hs.y, hs.z);
			verts[1] = point + new Vector3(hs.x, hs.y, -hs.z);
			verts[2] = point + new Vector3(hs.x, -hs.y, -hs.z);
			verts[3] = point + new Vector3(hs.x, -hs.y, hs.z);
			verts[4] = point + new Vector3(-hs.x, hs.y, hs.z);
			verts[5] = point + new Vector3(-hs.x, hs.y, -hs.z);
			verts[6] = point + new Vector3(-hs.x, -hs.y, -hs.z);
			verts[7] = point + new Vector3(-hs.x, -hs.y, hs.z);

			tempBoxSides.Add(new[] {verts[0], verts[1], verts[2], verts[3]});
			tempBoxSides.Add(new[] {verts[4], verts[5], verts[6], verts[7]});
			tempBoxSides.Add(new[] {verts[0], verts[4], verts[7], verts[3]});
			tempBoxSides.Add(new[] {verts[1], verts[5], verts[6], verts[2]});
			tempBoxSides.Add(new[] {verts[0], verts[1], verts[5], verts[4]});
			tempBoxSides.Add(new[] {verts[3], verts[2], verts[6], verts[7]});

			var prevZTest = Handles.zTest;
			Handles.zTest = DefaultZTest;

			foreach (var sideVertices in tempBoxSides)
				Handles.DrawSolidRectangleWithOutline(sideVertices, faceColor, outlineColor);

			Handles.zTest = prevZTest;

			tempBoxSides.Clear();
		}

		static Vector3 GetPointOnSphere(Vector3 center, float radius, float u, float v)
		{
			var theta = 2 * Mathf.PI * u;
			var phi = Mathf.PI * v;

			var x = center.x + radius * Mathf.Sin(phi) * Mathf.Cos(theta);
			var y = center.y + radius * Mathf.Sin(phi) * Mathf.Sin(theta);
			var z = center.z + radius * Mathf.Cos(phi);

			return new Vector3(x, y, z);
		}
	}
}