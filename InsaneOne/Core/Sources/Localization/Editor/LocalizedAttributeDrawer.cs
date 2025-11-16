using UnityEditor;
using UnityEngine;

namespace InsaneOne.Core.Locales.Editor
{
	[CustomPropertyDrawer(typeof(LocalizedAttribute))]
	public class LocalizedAttributeDrawer : PropertyDrawer
	{
		static readonly float MaxTextHeight = EditorGUIUtility.singleLineHeight;

		GUIStyle labelStyle;
		float extraHeight;
		string localizedText;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			position.height = EditorGUIUtility.singleLineHeight;
			EditorGUI.PropertyField(position, property, label);
			extraHeight = 0;

			var locale = property.stringValue;

			if (string.IsNullOrWhiteSpace(locale))
				return;

			if (!string.IsNullOrWhiteSpace(localizedText))
			{
				extraHeight = MaxTextHeight;
				var pos = position; // todo draw text correctly
				pos.y += MaxTextHeight;

				EditorGUI.LabelField(pos, localizedText, labelStyle);
				return;
			}

			labelStyle = new GUIStyle(EditorStyles.label) { fontSize = 10 };

			Localization.Initialize();
			Localization.TryGetText(locale, out localizedText);
			Localization.Unload();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return base.GetPropertyHeight(property, label) + extraHeight;
		}
	}
}