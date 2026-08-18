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

using UnityEditor;
using UnityEngine;

namespace InsaneOne.Core.LevelDesign
{
	/// <summary> Scene-view drawing helpers shared by ArrangeOnSceneWindow's Align/Distribute/Circle/Stack preview modes. </summary>
	static class ArrangePreviewDrawer
	{
		const float PreviewSphereSizeFactor = 0.25f;
		const float PreviewLineThickness = 2f;
		const float EndpointMarkerHeightFactor = 1.5f;
		const float EndpointMarkerSizeFactor = 0.2f;
		const float PositionUnchangedEpsilon = 0.0001f;

		public static readonly Color StartMarkerColor = new (0.3f, 1f, 0.4f, 0.9f);
		public static readonly Color EndMarkerColor = new (1f, 0.3f, 0.3f, 0.9f);

		static readonly Color PreviewSphereColor = new (0.3f, 0.85f, 1f, 0.6f);
		static readonly Color PreviewLineColor = new (0.3f, 0.85f, 1f, 0.25f);
		static readonly Color UnchangedPreviewSphereColor = new (0.6f, 0.6f, 0.6f, 0.6f);
		static readonly Color UnchangedPreviewLineColor = new (0.6f, 0.6f, 0.6f, 0.25f);

		/// <summary> Draws a line and a target-position sphere - gray when the target equals the current position
		/// (this object won't actually move), the usual accent color otherwise. </summary>
		public static void DrawPreviewPoint(Vector3 from, Vector3 to)
		{
			var unchanged = Vector3.Distance(from, to) < PositionUnchangedEpsilon;

			Handles.color = unchanged ? UnchangedPreviewLineColor : PreviewLineColor;
			Handles.DrawLine(from, to, PreviewLineThickness);

			Handles.color = unchanged ? UnchangedPreviewSphereColor : PreviewSphereColor;
			var size = HandleUtility.GetHandleSize(to) * PreviewSphereSizeFactor;
			Handles.SphereHandleCap(0, to, Quaternion.identity, size, EventType.Repaint);
		}

		/// <summary> Draws a small downward-pointing cone marker above the given position. </summary>
		public static void DrawEndpointMarker(Vector3 position, Color color)
		{
			var handleSize = HandleUtility.GetHandleSize(position);
			var markerPosition = position + Vector3.up * (handleSize * EndpointMarkerHeightFactor);

			Handles.color = color;
			Handles.ConeHandleCap(0, markerPosition, Quaternion.LookRotation(Vector3.down), handleSize * EndpointMarkerSizeFactor, EventType.Repaint);
		}

	}
}
