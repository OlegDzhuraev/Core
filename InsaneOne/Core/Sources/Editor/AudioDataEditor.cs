using UnityEditor;
using UnityEngine;

namespace InsaneOne.Core.Development
{
	[CustomPropertyDrawer(typeof(AudioData))]
	public sealed class AudioDataEditor : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.HelpBox(position, "", MessageType.None);
			
			var posOffsetted = new Rect(position.x + 16, position.y + 6, position.width - 32, position.height);
			EditorGUI.PropertyField(posOffsetted, property, label, true);
			
			var clipsProp = property.FindPropertyRelative("clipVariations");

			if (clipsProp.arraySize == 0 || clipsProp.GetArrayElementAtIndex(0).objectReferenceValue == null)
			{
				var prevColor = GUI.color;
				GUI.color = Color.yellow;
				
				const int w = 128;
				var pos = new Rect(position.x + position.width - w, position.y + 6, w, 16);

				EditorGUI.LabelField(pos, "No clips are set!");
				GUI.color = prevColor;
			}
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUI.GetPropertyHeight(property, label, true) + 12;
		}
	}
}