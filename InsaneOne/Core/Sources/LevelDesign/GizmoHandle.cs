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

using UnityEngine;

namespace InsaneOne.Core
{
	///<summary> Draws wire-gizmo with selected color and form. Useful to show primary objects in editor when them isn't selected.</summary>
	public class GizmoHandle : MonoBehaviour
	{
		enum Form
		{
			Box,
			Sphere
		}

		enum DrawType
		{
			Wired,
			WiredAndFilled
		}
		
		[SerializeField] Color gizmoColor = Color.red;
		[SerializeField] Form form = Form.Box;
		[SerializeField] DrawType drawType = DrawType.Wired;
		
		void OnDrawGizmos()
		{
			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.color = gizmoColor;

			if (form is Form.Sphere)
				DrawSphere(drawType);
			else
				DrawCube(drawType);
		}

		void DrawSphere(DrawType type)
		{
			var center = Vector3.zero;
			var radius = 0.5f;

			if (type is DrawType.WiredAndFilled)
				Gizmos.DrawSphere(center, radius);
			
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
	}
}