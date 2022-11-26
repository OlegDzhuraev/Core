using System;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace InsaneOne.Core.LevelDesign
{
	public class TransformRandomizerWindow : EditorWindow
	{
		[Flags]
		public enum Axes
		{
			None = 0,
			X = 1,
			Y = 2,
			Z = 4
		}
		
		float maxRotationAngle = 60;
		float minScale = 0.9f;
		float maxScale = 1.1f;
		float positionMaxTilt;
		
		Axes rotationAxes = Axes.X & Axes.Y & Axes.Z;
		Axes sizeAxes = Axes.X & Axes.Y & Axes.Z;
		Axes positionAxes = Axes.X & Axes.Y & Axes.Z;

		[MenuItem("Tools/Level Design/Transform Randomizer")]
		static void Init()
		{
			var window = (TransformRandomizerWindow)GetWindow(typeof(TransformRandomizerWindow), false, "Transform Randomizer", true);
			window.Show();
		}
		
		public void OnGUI()
		{
			rotationAxes = (Axes)EditorGUILayout.EnumFlagsField("Rotation Axes", rotationAxes);
			if (rotationAxes != Axes.None)
				maxRotationAngle = EditorGUILayout.Slider("Rotation randomize angle", maxRotationAngle, 0f, 180f);
			
			sizeAxes = (Axes)EditorGUILayout.EnumFlagsField("Size Axes", sizeAxes);
			if (sizeAxes != Axes.None)
			{
				GUILayout.Label("Scale random min max value");
				GUILayout.Label($"Min: {Math.Round(minScale, 3)}");
				GUILayout.Label($"Max: {Math.Round(maxScale, 3)}");
				EditorGUILayout.MinMaxSlider(ref minScale, ref maxScale, 0f, 10f);
			}

			positionAxes = (Axes)EditorGUILayout.EnumFlagsField("Position Axes", positionAxes);
			if (positionAxes != Axes.None)
				positionMaxTilt = EditorGUILayout.Slider("Position tilt shift max value", positionMaxTilt, 0f, 10f);

			// todo to keep position around start point we can add MonoBehaviour to the editing objects with initial data
			
			if (GUILayout.Button("Randomize"))
			{
				foreach (var obj in Selection.objects)
					if (obj is GameObject go)
						Randomize(go);
			}
		}

		void Randomize(GameObject go)
		{
			var tr = go.transform;
			
			if (rotationAxes != Axes.None)
				tr.Rotate(GetAxisValuedVector3(rotationAxes) * Random.Range(-maxRotationAngle, maxRotationAngle));

			if (sizeAxes != Axes.None)
			{
				var finalScale = tr.localScale;
				var scaleChange = GetAxisValuedVector3(sizeAxes) * Random.Range(minScale, maxScale);

				if (!Mathf.Approximately(scaleChange.x, 0))
					finalScale.x = scaleChange.x;	
				if (!Mathf.Approximately(scaleChange.y, 0))
					finalScale.y = scaleChange.y;	
				if (!Mathf.Approximately(scaleChange.z, 0))
					finalScale.z = scaleChange.z;
				
				tr.localScale = finalScale;
			}

			if (positionAxes != Axes.None)
				tr.position += GetAxisValuedVector3(positionAxes) * Random.Range(-positionMaxTilt, positionMaxTilt);
		}

		Vector3 GetAxisValuedVector3(Axes axes)
		{
			return new Vector3((axes & Axes.X) != 0 ? 1 : 0, (axes & Axes.Y) != 0 ? 1 : 0, (axes & Axes.Z) != 0 ? 1 : 0);
		}
	}
}