using System.IO;
using UnityEditor;
using UnityEngine;

namespace InsaneOne.Core.Development
{
	public class CoreInitEditorWindow : EditorWindow
	{
		public static void ShowWindow()
		{
			var wnd = GetWindow<CoreInitEditorWindow>();
			wnd.titleContent = new GUIContent("InsaneOne.Core Setup");
			wnd.minSize = new Vector2(340, 64);
			wnd.maxSize = new Vector2(340, 128);
		}

		void OnGUI()
		{
			if (CoreData.TryLoad(out _))
			{
				GUILayout.Label("Setup is finished!");

				if (GUILayout.Button("Close window"))
					Close();

				return;
			}

			GUILayout.Label("Looks like InsaneOne.Core was not setup before\n(No CoreData asset found).");

			var prevColor = GUI.color;
			GUI.color = Color.green;

			if (GUILayout.Button("Setup InsaneOne.Core"))
				Init();

			GUI.color = prevColor;
		}

		void Init()
		{
			var newData = ScriptableObject.CreateInstance<CoreData>();

			if (!AssetDatabase.IsValidFolder("Assets/Resources"))
				AssetDatabase.CreateFolder("Assets", "Resources");

			if (!AssetDatabase.IsValidFolder("Assets/Resources/InsaneOne"))
				AssetDatabase.CreateFolder("Assets/Resources", "InsaneOne");

			AssetDatabase.Refresh();
			AssetDatabase.CreateAsset(newData, "Assets/Resources/InsaneOne/CoreData.asset");

			CoreData.Log("new CoreData was <b><color=\"#55ff33\">created</color></b>.");
		}
	}
}