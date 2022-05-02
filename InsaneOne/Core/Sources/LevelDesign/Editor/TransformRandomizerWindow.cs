using System;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace InsaneOne.Core.LevelDesign
{
	public class TransformRandomizerWindow : EditorWindow
	{
		float maxRotationAngle = 60;
		float minScale = 0.9f;
		float maxScale = 1.1f;
		float positionMaxTilt;
		
		[MenuItem("Tools/Level Design/Transform Randomizer")]
		static void Init()
		{
			var window = (TransformRandomizerWindow)GetWindow(typeof(TransformRandomizerWindow), false, "Transform Randomizer", true);
			window.Show();
		}
		
		public void OnGUI()
		{
			maxRotationAngle = EditorGUILayout.Slider("Rotation randomize angle", maxRotationAngle, 0f, 180f);
			
			GUILayout.Label("Scale random min max value");
			GUILayout.Label($"Min: {Math.Round(minScale, 3)}");
			GUILayout.Label($"Max: {Math.Round(maxScale, 3)}");
			EditorGUILayout.MinMaxSlider(ref minScale, ref maxScale, 0f, 10f);
			
			positionMaxTilt = EditorGUILayout.Slider("Position tilt shift max value", positionMaxTilt, 0f, 10f);
			
			// todo flags for rotation random axes
			// todo flags for position axes
			
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
			go.transform.Rotate(0, Random.Range(-maxRotationAngle, maxRotationAngle), 0);
			go.transform.localScale = Vector3.one * Random.Range(minScale, maxScale);
			go.transform.position += new Vector3(Random.Range(-positionMaxTilt, positionMaxTilt), 0, Random.Range(-positionMaxTilt, positionMaxTilt));
		}
	}
}